// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Linq;
using System;
using Microsoft.CodeAnalysis.Text;
using System.Threading;

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{
    using System.Diagnostics;
    using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

    /// <summary>
    /// converts a source document into a XML node tree.
    /// </summary>
    internal class LanguageParser : SyntaxParser
    {
        // list pools - allocators for lists that are used to build sequences of nodes. The lists
        // can be reused (hence pooled) since the syntax factory methods don't keep references to
        // them

        private readonly SyntaxListPool _pool = new SyntaxListPool(); // Don't need to reset this.

        private readonly SyntaxFactoryContext _syntaxFactoryContext; // Fields are resettable.
        private readonly ContextAwareSyntax _syntaxFactory; // Has context, the fields of which are resettable.

        private TokenInfo _token;
        private List<SourceLocation> _skippedCharacters;
        private int _recursionDepth;

        private const int MaxDepth = 4000;

        public LanguageParser(
            Lexer scanner,
            Xaml.XamlSyntaxNode oldTree,
            IEnumerable<TextChangeRange> changes,
            CancellationToken cancellationToken = default(CancellationToken))
            : base(scanner, oldTree, changes, cancellationToken)
        {
            _syntaxFactoryContext = new SyntaxFactoryContext();
            _syntaxFactory = new ContextAwareSyntax(_syntaxFactoryContext);
        }

        internal TNode ParseWithStackGuard<TNode>(Func<TNode> parseFunc, Func<TNode> createEmptyNodeFunc) where TNode : XamlSyntaxNode
        {
            // If this value is non-zero then we are nesting calls to ParseWithStackGuard which should not be 
            // happening.  It's not a bug but it's inefficient and should be changed.
            Debug.Assert(_recursionDepth == 0);

            try
            {
                return parseFunc();
            }
            catch (Exception ex) when (StackGuard.IsInsufficientExecutionStackException(ex))
            {
                return CreateForGlobalFailure(_scanner.StartPos, createEmptyNodeFunc());
            }
        }

        private TNode CreateForGlobalFailure<TNode>(int position, TNode node) where TNode : XamlSyntaxNode
        {
            // Turn the complete input into a single skipped token. This avoids running the lexer,
            // which may itself run into the same problem that caused the original failure.
            var builder = new SyntaxListBuilder(1);
            builder.Add(SyntaxFactory.BadToken(null, _scanner.Text.ToString(), null));
            var fileAsTrivia = _syntaxFactory.SkippedTokensTrivia(builder.ToList<SyntaxToken>());
            node = AddLeadingSkippedSyntax(node, fileAsTrivia);
            ForceEndOfFile(); // force the scanner to report that it is at the end of the input.
            return AddError(node, position, 0, ErrorCode.MaxDepthReached);
        }

        public IList<SourceLocation> SkippedCharacters
        {
            get { return _skippedCharacters; }
        }

        private struct XamlBodyBuilder
        {
            public SyntaxListBuilder<XamlSyntaxNode> Elements;

            public XamlBodyBuilder(SyntaxListPool pool)
            {
                Elements = pool.Allocate<XamlSyntaxNode>();
            }

            internal void Free(SyntaxListPool pool)
            {
                pool.Free(Elements);
            }
        }

        internal XamlBodySyntax ParseXaml()
        {
            return ParseWithStackGuard(
                ParseXamlCore,
                () => SyntaxFactory.XamlBody(
                    new SyntaxList<XamlSyntaxNode>(),
                    SyntaxFactory.Token(SyntaxKind.EndOfFileToken)));
        }

        internal XamlBodySyntax ParseXamlCore()
        {
            XamlBodySyntax result = null;

            int start = _scanner.StartPos;
            int missingTagCount;
            NextToken();
            ResetDepth();
            bool hasNonWhitespace = false;
            var body = new XamlBodyBuilder(_pool);
            try
            {
                ParseXamlElementSyntaxBody(ref body, EndOfFile, out missingTagCount);
                ValidateDepth();
                Validate.Retail(missingTagCount == 0);
                var eof = this.EatToken(SyntaxKind.EndOfFileToken);
                result = _syntaxFactory.XamlBody(body.Elements, eof);
            }
            catch (InvalidOperationException)
            {
                hasNonWhitespace = true;
            }
            finally
            {
                body.Free(_pool);
            }

            //if (dummy.Nodes != null)
            //{
            //    foreach (XamlNodeSyntax node in dummy.Nodes)
            //    {
            //        if (node.SyntaxKind != SyntaxKind.Whitespace &&
            //            node.SyntaxKind != SyntaxKind.EndOfFile &&
            //            node.SyntaxKind != SyntaxKind.EndOfFile)
            //        {
            //            hasNonWhitespace = true;
            //            break;
            //        }
            //    }
            //}

            hasNonWhitespace = result.Elements.Any();

            if (!hasNonWhitespace) // Empty document
                ReportError(ErrorCode.EmptyDocument, 0, 0);

            //result.Range = GetTextRange(start, _scanner.EndPos - start);
            return result;
        }

        private void ParseXamlElementSyntaxBody(ref XamlBodyBuilder body, TokenSet followers, out int missingTagCount)
        {
            missingTagCount = 0;
            TokenSet followersOrXmlContent = followers | XmlContent;
            var builder = new SyntaxListBuilder(1);

            while (true)
            {
                switch (_token.Kind)
                {
                    case SyntaxKind.CharacterData:
                        builder.Clear(); // TODO:MGoertz
                        var cdata = _syntaxFactory.XamlCDataSection(SyntaxFactory.Token(SyntaxKind.XamlCDataStartToken), builder.ToList(), SyntaxFactory.Token(SyntaxKind.XamlCDataEndToken));
                        //node.Range = GetCurrentRange();
                        body.Elements.Add(cdata);
                        NextToken();
                        break;
                    case SyntaxKind.LiteralContentString:
                        //node.Range = GetCurrentRange();
                        //if (_scanner.Whitespace)
                        //    node.SyntaxKind = SyntaxKind.Whitespace;
                        //else
                        //    node.SyntaxKind = _token;
                        var content = SyntaxFactory.XamlTextLiteral(null, _token.Text, _token.Text, null);
                        body.Elements.Add(content);
                        NextToken();
                        break;

                    case SyntaxKind.StartOfRegion:
                    case SyntaxKind.EndOfRegion:
                    case SyntaxKind.ProcessingInstruction:
                    case SyntaxKind.Comment:
                        //node.Range = GetCurrentRange();
                        //node.SyntaxKind = _token;
                        //node.Value = null;
                        //AddChild(element, node);
                        var node = SyntaxFactory.Token(null, _token.Kind, null, null, null);
                        body.Elements.Add(node);
                        NextToken();
                        break;

                    case SyntaxKind.StartOfTag:
                        IncreaseAndCheckDepth();
                        XamlNodeSyntax childElement = ParseXamlElement(followersOrXmlContent, out missingTagCount);
                        DecreaseDepth();
                        //node.Range = childElement.Range;
                        body.Elements.Add(childElement);
                        if (missingTagCount > 0)
                        {
                            //FixWhitespace(element);
                            return; // Do not continue parsing the fragment if a closing tag matching a parent element name has been encountered
                        }
                        break;

                    default:
                        if (followers[_token.Kind])
                        {
                            //FixWhitespace(element);
                            return;
                        }
                        SkipTo(followers);
                        break;
                }
            }
        }

        // Ensure leading text with leading whitespace and trailing text with trailing whitespace are both
        // split into whitespace/non-whitespace nodes.
        //private static void FixWhitespace(XamlElementSyntax element)
        //{
        //    if (element.Nodes != null && element.Nodes.Count > 0)
        //    {
        //        for (int i = 0; i < element.Nodes.Count; i++)
        //        {
        //            XmlNode node = element.Nodes[i];
        //            if (node.SyntaxKind == SyntaxKind.LiteralContentString)
        //            {
        //                string text = node.Value as string;
        //                if (text != null)
        //                {
        //                    if (text.Length != node.Range.Length)
        //                        continue; // We have entities mapped in here so without going back to the source we can't adjust ranges safely
        //                    int j = 0;
        //                    while (j < text.Length)
        //                        if (!Lexer.IsWhitespaceOrEol(text[j++]))
        //                        {
        //                            j--;
        //                            break;
        //                        }
        //                    if (j > 0 && j < text.Length)
        //                    {
        //                        XmlNode whiteNode = new XmlNode();
        //                        whiteNode.SyntaxKind = SyntaxKind.Whitespace;
        //                        whiteNode.Range = new SourceLocation(node.Range.Location, j);
        //                        whiteNode.Value = text.Substring(0, j);
        //                        text = text.Substring(j);
        //                        node.Value = text;
        //                        node.Range = new SourceLocation(node.Range.Location + j, node.Range.Length - j);
        //                        element.Nodes[i] = node;
        //                        element.Nodes.Insert(i, whiteNode);
        //                        i++;
        //                    }
        //                    j = text.Length;
        //                    while (j > 0)
        //                        if (!Lexer.IsWhitespaceOrEol(text[--j]))
        //                        {
        //                            j++;
        //                            break;
        //                        }
        //                    if (j > 0 && j < text.Length)
        //                    {
        //                        XmlNode whiteNode = new XmlNode();
        //                        whiteNode.SyntaxKind = SyntaxKind.Whitespace;
        //                        int end = node.Range.Location + node.Range.Length;
        //                        int start = end - text.Length + j;
        //                        whiteNode.Range = new SourceLocation(start, end - start);
        //                        whiteNode.Value = text.Substring(j);
        //                        node.Value = text.Substring(0, j);
        //                        node.Range = new SourceLocation(node.Range.Location, node.Range.Length - whiteNode.Range.Length);
        //                        element.Nodes[i] = node;
        //                        i++;
        //                        element.Nodes.Insert(i, whiteNode);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        private XamlNodeSyntax ParseXamlElement(TokenSet followers, out int missingTagCount)
        {
            XamlNameSyntax name = null;
            int startPos = _scanner.StartPos;
            int startLine = _scanner.Line;

            Validate.Retail(_token.Kind == SyntaxKind.StartOfTag);
            var lessThan = SyntaxFactory.Token(SyntaxKind.LessThanToken);
            NextToken();
            if (_scanner.WasWhitespaceSkipped)
            {
                name = _syntaxFactory.XamlName(null, null);
                //result.Name = new XmlQName();
                //result.Name.Name = Identifier.For(null);
                //result.Name.Range = GetTextRange(startPos + 1, 0);
                ReportError(ErrorCode.IllegalWhitespaceInIdentifier, startPos + 1, _scanner.EndPos - (startPos + 1));
            }
            else
            {
                name = this.ParseXamlName();
            }

            missingTagCount = 0;
            var attrs = _pool.Allocate<XamlAttributeSyntax>();
            try
            {
                this.ParseXamlAttributes(ref name, attrs);

                if (this.CurrentToken.Kind == SyntaxKind.GreaterThanToken)
                {
                    var startTag = SyntaxFactory.XamlElementStartTag(lessThan, name, attrs, this.EatToken());
                    //this.SetMode(LexerMode.XmlDocComment);
                    var body = new XamlBodyBuilder(_pool);
                    try
                    {
                        IncreaseAndCheckDepth();
                        ParseXamlElementSyntaxBody(ref body, followers | SyntaxKind.StartOfClosingTag, out missingTagCount);
                        DecreaseDepth();

                        XamlNameSyntax endName;
                        SyntaxToken greaterThan;

                        // end tag
                        var lessThanSlash = this.EatToken(SyntaxKind.LessThanSlashToken, reportError: false);

                        // If we didn't see "</", then we can't really be confident that this is actually an end tag,
                        // so just insert a missing one.
                        if (lessThanSlash.IsMissing)
                        {
                            //this.ResetMode(saveMode);
                            lessThanSlash = this.WithXamlParseError(lessThanSlash, ErrorCode.ClosingTagMismatch, name.ToString());
                            endName = SyntaxFactory.XamlName(prefix: null, localName: SyntaxFactory.MissingToken(SyntaxKind.IdentifierToken));
                            greaterThan = SyntaxFactory.MissingToken(SyntaxKind.GreaterThanToken);
                        }
                        else
                        {
                            //this.SetMode(LexerMode.XmlElementTag);
                            endName = this.ParseXamlName();
                            if (lessThanSlash.GetTrailingTriviaWidth() > 0 || endName.GetLeadingTriviaWidth() > 0)
                            {
                                // The Xml spec disallows whitespace here: STag ::= '<' Name (S Attribute)* S? '>' 
                                endName = this.WithXamlParseError(endName, ErrorCode.IllegalWhitespaceInIdentifier);
                            }

                            if (!endName.IsMissing && !MatchingXamlNames(name, endName))
                            {
                                endName = this.WithXamlParseError(endName, ErrorCode.ClosingTagMismatch, endName.ToString(), name.ToString());
                            }

                            // if we don't see the greater than token then skip the badness until we do or abort
                            if (this.CurrentToken.Kind != SyntaxKind.GreaterThanToken)
                            {
                                this.SkipBadTokens(ref endName, null,
                                    p => p.CurrentToken.Kind != SyntaxKind.GreaterThanToken,
                                    p => p.IsXamlNodeStartOrStop(),
                                    ErrorCode.UnexpectedToken
                                    );
                            }

                            greaterThan = this.EatToken(SyntaxKind.GreaterThanToken);
                        }

                        var endTag = SyntaxFactory.XamlElementEndTag(lessThanSlash, endName, greaterThan);
                        //this.ResetMode(saveMode);
                        return SyntaxFactory.XamlElement(startTag, body.Elements.ToList(), endTag);
                    }
                    finally
                    {
                        body.Free(_pool);
                    }
                }
                else
                {
                    var slashGreater = this.EatToken(SyntaxKind.SlashGreaterThanToken, false);
                    if (slashGreater.IsMissing && !name.IsMissing)
                    {
                        slashGreater = this.WithXamlParseError(slashGreater, ErrorCode.ClosingTagMismatch, name.ToString());
                    }

                    return SyntaxFactory.XamlEmptyElement(lessThan, name, attrs, slashGreater);
                }
            }
            finally
            {
                _pool.Free(attrs);
            }
        }

        //private void ParseXmlStartTag(XamlElementSyntax element, int startPos, int startLine, out int endPos, out bool expectEndTag)
        //{
        //    if (!_scanner.WasWhitespaceSkipped)
        //    {
        //        element.Name = ParseQualifiedName();
        //    }

        //    if (_token == SyntaxKind.Identifier)
        //    {
        //        element.Attributes = new List<XamlAttributeSyntax>();
        //        while (_token == SyntaxKind.Identifier)
        //        {
        //            // Need to have whitespace between attributes
        //            if (!_scanner.IsWhitespaceOrEolBeforeStartingChar)
        //                ReportError(ErrorCode.ExpectedWhitespace, _scanner.StartPos, _scanner.EndPos - _scanner.StartPos, _scanner.GetTokenSource());
        //            element.Attributes.Add(ParseXamlAttributeSyntax());
        //        }
        //    }

        //    //endPos = _scanner.EndPos;
        //    //if (_token == SyntaxKind.StartOfTag || _token == SyntaxKind.StartOfClosingTag)
        //    //{
        //    //    XamlAttributeSyntax attribute = element.Attributes?.LastOrDefault();
        //    //    if (attribute != null && attribute.IsValueProperlyStarted && !attribute.IsValueProperlyTerminated)
        //    //    {
        //    //        // The string value is not closed properly
        //    //        // Use the end position of last attribute as the tag end position
        //    //        endPos = attribute.Range.EndOfRange;
        //    //    }
        //    //}

        //    //element.StartTagRange = GetTextRange(startPos, endPos - startPos);
        //    if (_token == SyntaxKind.EndOfSimpleTag)
        //    {
        //        //element.IsSimpleTag = true;
        //        //element.IsSingleLine = startLine == _scanner.Line;
        //        NextToken();
        //        expectEndTag = false;
        //    }
        //    else
        //    {
        //        if (_token == SyntaxKind.EndOfTag)
        //            NextToken();
        //        else
        //            ReportError(ErrorCode.ExpectedDifferentToken, _scanner.StartPos, _scanner.EndPos - _scanner.StartPos, ">");
        //        expectEndTag = true;
        //    }
        //}

        //private void ParseXmlEndTag(XamlElementSyntax element, out int endPos, out int missingTagCount)
        //{
        //    missingTagCount = 0;
        //    XamlElementSyntax parent = element.Parent;
        //    XmlQName tagName = element.Name;
        //    int startPos = _scanner.StartPos;
        //    Validate.Retail(_token == SyntaxKind.StartOfClosingTag);
        //    NextToken();
        //    bool skippedLeadingWhitepace;
        //    XmlQName endTagName = ParseQualifiedName(out skippedLeadingWhitepace);
        //    endPos = _scanner.EndPos;
        //    element.EndTagRange = GetTextRange(startPos, endPos - startPos);
        //    element.EndNameRange = endTagName.Range;
        //    element.IsEndTagComplete = true;
        //    if (tagName.Prefix == endTagName.Prefix && tagName.Name == endTagName.Name)
        //    {

        //        // If the token is incorrect, we limit the text range to incomplete tag.
        //        // This too is for intellisense purposes.
        //        if (_token != SyntaxKind.EndOfTag)
        //        {//</Window
        //            element.EndTagRange = GetTextRange(startPos, endTagName.Range.Length + 2);
        //            endPos = element.EndTagRange.Location + element.EndTagRange.Length;
        //            element.IsEndTagComplete = false;
        //        }

        //        Skip(SyntaxKind.EndOfTag, ">");
        //    }
        //    else
        //    {
        //        // Updating the end tag range to be right after the end of the qualified name
        //        // This is solely done for the language service. 
        //        if (endTagName.Range.Length == 0 || _token != SyntaxKind.EndOfTag)
        //        { // The "</Window" case
        //            element.EndTagRange = GetTextRange(startPos, endTagName.Range.Length + 2);
        //            element.IsEndTagComplete = false;
        //            if (endTagName.Range.Length == 0 || skippedLeadingWhitepace)
        //            {
        //                // If the end tag doesn't have a name (i.e. it is just "</") then set the 
        //                // range to the end of the </ and the length to 0.
        //                element.EndNameRange.Location = startPos + 2;
        //                element.EndNameRange.Length = 0;
        //            }
        //            else
        //                element.EndNameRange = endTagName.Range;
        //        }

        //        while (true)
        //        {
        //            missingTagCount++;
        //            XmlQName parentName = parent != null ? parent.Name : null;
        //            if (parentName != null && endTagName.Prefix == parentName.Prefix && endTagName.Name == parentName.Name)
        //            {
        //                ReportError(ErrorCode.ClosingTagMismatch, startPos, _scanner.EndPos - startPos, tagName.ToString());
        //                // This is likely not our end tag, clear our end tag and set the matching parent endTag information
        //                parent.IsEndTagComplete = _token == SyntaxKind.EndOfTag;
        //                parent.EndTagRange = element.EndTagRange;
        //                parent.EndNameRange = element.EndNameRange;
        //                element.EndTagRange.Location--;
        //                element.EndTagRange.Length = 0;
        //                element.EndNameRange.Location = 0;
        //                element.EndNameRange.Length = 0;
        //                element.IsEndTagComplete = false;
        //            }
        //            else if (parent != null && parent.Parent != null)
        //            {
        //                parent = parent.Parent;
        //                continue;
        //            }
        //            else
        //            {
        //                //The closing tag does not match the starting tag of this element nor any of its parents. Skip over it.
        //                missingTagCount = 0;
        //                Skip(SyntaxKind.EndOfTag, ">");
        //                ReportError(ErrorCode.ClosingTagMismatch, startPos, _scanner.EndPos - startPos, tagName.ToString());
        //            }
        //            break;
        //        }
        //    }
        //}

        //private XamlAttributeSyntax ParseXamlAttributeSyntax()
        //{
        //    XamlAttributeSyntax attribute = new XamlAttributeSyntax();
        //    int startPos = _scanner.StartPos;
        //    attribute.Name = ParseQualifiedName();
        //    int endPos = _scanner.EndPos;
        //    if (_token != SyntaxKind.Assign)
        //        // If this will be a syntax error, don't include this token in the range.
        //        if (attribute.Name != null)
        //            // Ignore potential whitespace.
        //            endPos = attribute.Name.Range.Location + attribute.Name.Range.Length;
        //        else
        //            endPos = _scanner.StartPos;
        //    Skip(SyntaxKind.Assign, "=");
        //    bool unquotedValue = false;
        //    if (_token == SyntaxKind.StringLiteral)
        //    {
        //        attribute.Value = new XmlValue();
        //        attribute.Value.Range = GetCurrentRange();
        //        attribute.Value.Value = _scanner.GetStringLiteral();
        //        attribute.IsValueProperlyTerminated = _scanner.StringLiteralTerminatedProperly;
        //        attribute.IsValueProperlyStarted = true; // It wouldn't be detected as a string literal without this
        //        endPos = _scanner.EndPos;
        //    }
        //    else if (_token == SyntaxKind.Identifier && !_scanner.WasWhitespaceSkipped)
        //    {
        //        // This case is where attribute=value and the quotes are missing, assume
        //        // that is what is intended and allow the missing quotes error to show and intellisense
        //        // to work against this as if it were a value
        //        attribute.Value = new XmlValue();
        //        attribute.Value.Range = GetCurrentRange();
        //        attribute.Value.Value = _scanner.GetTokenSource();
        //        attribute.IsValueProperlyStarted = false;
        //        endPos = _scanner.EndPos;
        //        unquotedValue = true;
        //    }
        //    attribute.Range = GetTextRange(startPos, endPos - startPos);
        //    Skip(SyntaxKind.StringLiteral, ErrorCode.ExpectedString);
        //    if (unquotedValue)
        //        NextToken(); // we allow the skip above to report the error, and since we know this isn't the EOF we just skip to the next token
        //    return attribute;
        //}

        private bool IsXamlNodeStartOrStop()
        {
            switch (this.CurrentToken.Kind)
            {
                case SyntaxKind.LessThanToken:
                case SyntaxKind.LessThanSlashToken:
                case SyntaxKind.Comment:             //case SyntaxKind.XamlCommentStartToken:
                case SyntaxKind.XamlCDataStartToken: //case SyntaxKind.XamlProcessingInstructionStartToken:
                case SyntaxKind.ProcessingInstruction:
                case SyntaxKind.GreaterThanToken:
                case SyntaxKind.SlashGreaterThanToken:
                    return true;
                default:
                    return false;
            }
        }

        private XamlNameSyntax ParseXamlName()
        {
            var id = this.EatToken(SyntaxKind.IdentifierToken);
            XamlPrefixSyntax prefix = null;
            if (this.CurrentToken.Kind == SyntaxKind.ColonToken)
            {
                var colon = this.EatToken();

                int prefixTrailingWidth = id.GetTrailingTriviaWidth();
                int colonLeadingWidth = colon.GetLeadingTriviaWidth();

                if (prefixTrailingWidth > 0 || colonLeadingWidth > 0)
                {
                    // NOTE: offset is relative to full-span start of colon (i.e. before leading trivia).
                    int offset = -prefixTrailingWidth;
                    int width = prefixTrailingWidth + colonLeadingWidth;
                    colon = WithAdditionalDiagnostics(colon, new SyntaxDiagnosticInfo(offset, width, ErrorCode.IllegalWhitespaceInIdentifier));
                }

                prefix = SyntaxFactory.XamlPrefix(id, colon);
                id = this.EatToken(SyntaxKind.IdentifierToken);

                int colonTrailingWidth = colon.GetTrailingTriviaWidth();
                int localNameLeadingWidth = id.GetLeadingTriviaWidth();
                if (colonTrailingWidth > 0 || localNameLeadingWidth > 0)
                {
                    // NOTE: offset is relative to full-span start of identifier (i.e. before leading trivia).
                    int offset = -colonTrailingWidth;
                    int width = colonTrailingWidth + localNameLeadingWidth;
                    id = WithAdditionalDiagnostics(id, new SyntaxDiagnosticInfo(offset, width, ErrorCode.IllegalWhitespaceInIdentifier));

                    // CONSIDER: Another interpretation would be that the local part of this name is a missing identifier and the identifier
                    // we've just consumed is actually part of something else (e.g. an attribute name).
                }
            }

            return _syntaxFactory.XamlName(prefix, id);
        }

        private static bool MatchingXamlNames(XamlNameSyntax name, XamlNameSyntax endName)
        {
            // PERF: because of deduplication we often get the same name for name and endName,
            //       so we will check for such case first before materializing text for entire nodes 
            //       and comparing that.
            if (name == endName)
            {
                return true;
            }

            // before doing ToString, check if 
            // all nodes contributing to ToString are recursively the same
            // NOTE: leading and trailing trivia do not contribute to ToString
            if (!name.HasLeadingTrivia &&
                !endName.HasTrailingTrivia &&
                name.IsEquivalentTo(endName))
            {
                return true;
            }

            return name.ToString() == endName.ToString();
        }

        // assuming this is not used concurrently
        private readonly HashSet<string> _attributesSeen = new HashSet<string>();

        private void ParseXamlAttributes(ref XamlNameSyntax elementName, SyntaxListBuilder<XamlAttributeSyntax> attrs)
        {
            _attributesSeen.Clear();
            while (true)
            {
                if (this.CurrentToken.Kind == SyntaxKind.IdentifierToken)
                {
                    var attr = this.ParseXamlAttribute(elementName);
                    string attrName = attr.Name.ToString();
                    if (_attributesSeen.Contains(attrName))
                    {
                        attr = this.WithXamlParseError(attr, ErrorCode.AttributeAlreadyDefined, attrName);
                    }
                    else
                    {
                        _attributesSeen.Add(attrName);
                    }

                    attrs.Add(attr);
                }
                else
                {
                    var abort = this.SkipBadTokens(ref elementName, attrs,

                        // not expected condition
                        p => p.CurrentToken.Kind != SyntaxKind.Identifier,

                        // abort condition (looks like something we might understand later)
                        p => p.CurrentToken.Kind == SyntaxKind.GreaterThanToken
                            || p.CurrentToken.Kind == SyntaxKind.SlashGreaterThanToken
                            || p.CurrentToken.Kind == SyntaxKind.LessThanToken
                            || p.CurrentToken.Kind == SyntaxKind.LessThanSlashToken
                            || p.CurrentToken.Kind == SyntaxKind.EndOfFileToken,

                        ErrorCode.UnexpectedToken
                    );

                    if (abort)
                    {
                        break;
                    }
                }
            }
        }

        private XamlAttributeSyntax ParseXamlAttribute(XamlNameSyntax elementName)
        {
            var attrName = this.ParseXamlName();
            if (attrName.GetLeadingTriviaWidth() == 0)
            {
                // The Xml spec requires whitespace here: STag ::= '<' Name (S Attribute)* S? '>' 
                attrName = this.WithXamlParseError(attrName, ErrorCode.ExpectedWhitespace);
            }

            var equals = this.EatToken(SyntaxKind.EqualsToken, false);
            if (equals.IsMissing)
            {
                equals = this.WithXamlParseError(equals, ErrorCode.ExpectedDifferentToken, "=");

                switch (this.CurrentToken.Kind)
                {
                    case SyntaxKind.SingleQuoteToken:
                    case SyntaxKind.DoubleQuoteToken:
                        // There could be a value coming up, let's keep parsing.
                        break;
                    default:
                        // This is probably not a complete attribute.
                        return SyntaxFactory.XamlTextAttribute(
                            attrName,
                            equals,
                            SyntaxFactory.MissingToken(SyntaxKind.DoubleQuoteToken),
                            default(SyntaxList<SyntaxToken>),
                            SyntaxFactory.MissingToken(SyntaxKind.DoubleQuoteToken));
                }
            }

            SyntaxToken startQuote;
            SyntaxToken endQuote;
            string attrNameText = attrName.LocalName.ValueText;
            var textTokens = _pool.Allocate<SyntaxToken>();
            try
            {
                this.ParseXamlAttributeText(out startQuote, textTokens, out endQuote);
                return SyntaxFactory.XamlTextAttribute(attrName, equals, startQuote, textTokens, endQuote);
            }
            finally
            {
                _pool.Free(textTokens);
            }
        }

        private void ParseXamlAttributeText(out SyntaxToken startQuote, SyntaxListBuilder<SyntaxToken> textTokens, out SyntaxToken endQuote)
        {
            startQuote = ParseXmlAttributeStartQuote();
            SyntaxKind quoteKind = startQuote.Kind;

            // NOTE: Being a bit sneaky here - if the width isn't 0, we consumed something else in
            // place of the quote and we should continue parsing the attribute.
            if (startQuote.IsMissing && startQuote.FullWidth == 0)
            {
                endQuote = SyntaxFactory.MissingToken(quoteKind);
            }
            else
            {
                //var saveMode = this.SetMode(quoteKind == SyntaxKind.SingleQuoteToken
                //    ? LexerMode.XmlAttributeTextQuote
                //    : LexerMode.XmlAttributeTextDoubleQuote);

                while (this.CurrentToken.Kind == SyntaxKind.StringLiteral
                    || this.CurrentToken.Kind == SyntaxKind.EndOfLine)
                {
                    var token = this.EatToken();

                    textTokens.Add(token);
                }

                // TODO:MGoertz Don't we need the text?
                //_scanner.GetStringLiteral();

                //this.ResetMode(saveMode);

                // NOTE: This will never consume a non-ascii quote, since non-ascii quotes
                // are legal in the attribute value and are consumed by the preceding loop.
                endQuote = ParseXmlAttributeEndQuote(quoteKind);
            }
        }

        private SyntaxToken ParseXmlAttributeStartQuote()
        {
            if (IsNonAsciiQuotationMark(this.CurrentToken))
            {
                return SkipNonAsciiQuotationMark();
            }

            var quoteKind = this.CurrentToken.Kind == SyntaxKind.SingleQuoteToken
                ? SyntaxKind.SingleQuoteToken
                : SyntaxKind.DoubleQuoteToken;

            var startQuote = this.EatToken(quoteKind, reportError: false);
            if (startQuote.IsMissing)
            {
                startQuote = this.WithXamlParseError(startQuote, ErrorCode.ExpectedDifferentToken, "\"");
            }
            return startQuote;
        }

        private SyntaxToken ParseXmlAttributeEndQuote(SyntaxKind quoteKind)
        {
            if (IsNonAsciiQuotationMark(this.CurrentToken))
            {
                return SkipNonAsciiQuotationMark();
            }

            var endQuote = this.EatToken(quoteKind, reportError: false);
            if (endQuote.IsMissing)
            {
                endQuote = this.WithXamlParseError(endQuote, ErrorCode.ExpectedDifferentToken, "\"");
            }
            return endQuote;
        }

        private SyntaxToken SkipNonAsciiQuotationMark()
        {
            var quote = SyntaxFactory.MissingToken(SyntaxKind.DoubleQuoteToken);
            quote = AddTrailingSkippedSyntax(quote, EatToken());
            quote = this.WithXamlParseError(quote, ErrorCode.ExpectedDifferentToken, "\"");
            return quote;
        }

        /// <summary>
        /// These aren't acceptable in place of ASCII quotation marks in XML, 
        /// but we want to consume them (and produce an appropriate error) if
        /// they occur in a place where a quotation mark is legal.
        /// </summary>
        private static bool IsNonAsciiQuotationMark(SyntaxToken token)
        {
            return token.Text.Length == 1 && SyntaxFacts.IsNonAsciiQuotationMark(token.Text[0]);
        }

        private bool SkipBadTokens<T>(
            ref T startNode,
            SyntaxListBuilder list,
            Func<LanguageParser, bool> isNotExpectedFunction,
            Func<LanguageParser, bool> abortFunction,
            ErrorCode error
            ) where T : XamlSyntaxNode
        {
            var badTokens = default(SyntaxListBuilder<SyntaxToken>);
            bool hasError = false;

            try
            {
                bool abort = false;

                while (isNotExpectedFunction(this))
                {
                    if (abortFunction(this))
                    {
                        abort = true;
                        break;
                    }

                    if (badTokens.IsNull)
                    {
                        badTokens = _pool.Allocate<SyntaxToken>();
                    }

                    var token = this.EatToken();
                    if (!hasError)
                    {
                        token = this.WithXamlParseError(token, error, token.ToString());
                        hasError = true;
                    }

                    badTokens.Add(token);
                }

                if (!badTokens.IsNull && badTokens.Count > 0)
                {
                    // use skipped text since cannot nest structured trivia under structured trivia
                    if (list == null || list.Count == 0)
                    {
                        startNode = AddTrailingSkippedSyntax(startNode, badTokens.ToListNode());
                    }
                    else
                    {
                        list[list.Count - 1] = AddTrailingSkippedSyntax((XamlSyntaxNode)list[list.Count - 1], badTokens.ToListNode());
                    }

                    return abort;
                }
                else
                {
                    // somehow we did not consume anything, so tell caller to abort parse rule
                    return true;
                }
            }
            finally
            {
                if (!badTokens.IsNull)
                {
                    _pool.Free(badTokens);
                }
            }
        }

        //[SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        //public static string ParseName(string name, out bool skippedLeadingWhitespace)
        //{
        //    skippedLeadingWhitespace = false;
        //    if (string.IsNullOrEmpty(name))
        //    {
        //        return string.Empty;
        //    }

        //    var scanner = new Lexer(new ScannerProviderAdapter(new StringSourceReader(name)), delegate { });
        //    var parser = new LanguageParser(scanner, null);
        //    scanner.ScannerState = (int)State.Tag;
        //    parser.NextToken();
        //    XmlQName qname = parser.ParseQualifiedName(out skippedLeadingWhitespace);
        //    StringBuilder sb = new StringBuilder();
        //    if (qname.Prefix.IsDefined)
        //    {
        //        sb.Append(qname.Prefix.Name);
        //        sb.Append(':');
        //    }
        //    if (qname.Name.IsDefined)
        //    {
        //        sb.Append(qname.Name.Name);
        //    }
        //    return sb.ToString();
        //}

        //internal static XmlQName ParseQualifiedName(string name)
        //{
        //    var scanner = new Lexer(new ScannerProviderAdapter(new StringSourceReader(name)), delegate { });
        //    var parser = new LanguageParser(scanner, null);
        //    scanner.ScannerState = (int)State.Tag;
        //    parser.NextToken();
        //    return parser.ParseQualifiedName();
        //}

        //private XmlQName ParseQualifiedName()
        //{
        //    bool skippedLeadingWhitespace;
        //    return ParseQualifiedName(out skippedLeadingWhitespace);
        //}

        //private XmlQName ParseQualifiedName(out bool skippedLeadingWhitespace)
        //{
        //    skippedLeadingWhitespace = false;
        //    XmlQName result = new XmlQName();
        //    int startPos = _scanner.StartPos;
        //    int endPos = _scanner.EndPos;
        //    string identifierName;
        //    if (_token != SyntaxKind.Identifier)
        //    {
        //        // If we are going to report an error, don't update the endPos
        //        endPos = startPos;
        //        // Leave the identifier blank
        //        identifierName = "";
        //    }
        //    else
        //    {
        //        identifierName = _scanner.GetTokenSource();
        //        // Since Skip will cause the WasWhitepaceSkipped property to be reset based on
        //        // this latest operation, we store what happened in this initial scanner operation
        //        if (_scanner.WasWhitespaceSkipped)
        //            skippedLeadingWhitespace = true;
        //    }
        //    Skip(SyntaxKind.Identifier, ErrorCode.ExpectedIdentifier);
        //    if (_token == SyntaxKind.Colon && !_scanner.WasWhitespaceSkipped)
        //    {
        //        result.Prefix = Identifier.For(identifierName);
        //        endPos = _scanner.EndPos;
        //        NextToken();
        //        if (!_scanner.WasWhitespaceSkipped)
        //        {
        //            if (_token == SyntaxKind.Identifier)
        //            {
        //                identifierName = _scanner.GetTokenSource();
        //                endPos = _scanner.EndPos;
        //            }
        //            else
        //                identifierName = null;
        //            Skip(SyntaxKind.Identifier, ErrorCode.ExpectedIdentifier);
        //        }
        //        else
        //            identifierName = null;
        //    }
        //    if (identifierName != null)
        //        result.Name = Identifier.For(identifierName);
        //    result.Range = GetTextRange(startPos, endPos - startPos);

        //    return result;
        //}

        /// TODO:MGoertz
        #region TODO:MGoertz - Hide base members
        private SyntaxToken CreateMissingToken(SyntaxKind expected, SyntaxKind actual, bool reportError)
        {
            // should we eat the current ParseToken's leading trivia?
            var token = SyntaxFactory.MissingToken(expected);
            if (reportError)
            {
                token = WithAdditionalDiagnostics(token, this.GetExpectedTokenError(expected, actual));
            }

            return token;
        }

        private SyntaxToken CreateMissingToken(SyntaxKind expected, ErrorCode code, bool reportError)
        {
            // should we eat the current ParseToken's leading trivia?
            var token = SyntaxFactory.MissingToken(expected);
            if (reportError)
            {
                token = AddError(token, code);
            }

            return token;
        }

        protected new SyntaxToken CurrentToken
        {
            get
            {
                var ct = _scanner.Create(ref _token, null, null, null);
                return ct;
            }
        }

        protected new SyntaxToken EatToken()
        {
            var ct = _scanner.Create(ref _token, null, null, null);
            NextToken();
            return ct;
        }

        protected new SyntaxToken EatToken(SyntaxKind kind)
        {
            Debug.Assert(SyntaxFacts.IsAnyToken(kind));

            var ct = _scanner.Create(ref _token, null, null, null);
            if (ct.Kind == kind)
            {
                NextToken();
                return ct;
            }

            //slow part of EatToken(SyntaxKind kind)
            return CreateMissingToken(kind, ct.Kind, reportError: true);
        }

        protected new SyntaxToken EatToken(SyntaxKind kind, bool reportError)
        {
            if (reportError)
            {
                return EatToken(kind);
            }

            Debug.Assert(SyntaxFacts.IsAnyToken(kind));
            if (this.CurrentToken.Kind != kind)
            {
                // should we eat the current ParseToken's leading trivia?
                return SyntaxFactory.MissingToken(kind);
            }
            else
            {
                return this.EatToken();
            }
        }

        protected new SyntaxToken EatToken(SyntaxKind kind, ErrorCode code, bool reportError = true)
        {
            Debug.Assert(SyntaxFacts.IsAnyToken(kind));
            if (_token.Kind != kind)
            {
                return CreateMissingToken(kind, code, reportError);
            }
            else
            {
                return this.EatToken();
            }
        }

        #endregion

        private SyntaxKind NextToken()
        {
            _token = _scanner.GetNextToken();
            return _token.Kind;
        }

        private void Skip(SyntaxKind token, ErrorCode error)
        {
            if (_token.Kind == token)
            {
                if (token != SyntaxKind.EndOfFile)
                    NextToken();
            }
            else
                this.ReportError(error, _scanner.StartPos, _scanner.EndPos - _scanner.StartPos);
        }

        private void Skip(SyntaxKind token, string expectedToken)
        {
            if (_token.Kind == token)
            {
                if (token != SyntaxKind.EndOfFile)
                    NextToken();
            }
            else
                this.ReportError(ErrorCode.ExpectedDifferentToken, _scanner.StartPos, _scanner.EndPos - _scanner.StartPos, expectedToken);
        }

        private void SkipTo(TokenSet followers)
        {
            if (followers[_token.Kind])
                return;
            ReportError(ErrorCode.UnexpectedToken, _scanner.StartPos, _scanner.EndPos - _scanner.StartPos, _scanner.GetTokenSource());

            Validate.Debug(followers[SyntaxKind.EndOfFile]);

            int start = _scanner.StartPos;
            while (!followers[NextToken()])
                ;
            ReportSkipped(start, _scanner.StartPos - start);
        }

        private void ReportSkipped(int location, int length)
        {
            //if (_skippedCharacters == null)
            //    _skippedCharacters = new List<SourceLocation>();
            //SourceLocation newLocation = new SourceLocation(location, length);
            //if (_skippedCharacters.Count < 1 || _skippedCharacters[_skippedCharacters.Count - 1] != newLocation)
            //    _skippedCharacters.Add(newLocation);
        }

        //private SourceLocation GetCurrentRange()
        //{
        //    return new SourceLocation(_scanner.StartPos, _scanner.EndPos - _scanner.StartPos);
        //}

        //private static SourceLocation GetTextRange(int start, int length)
        //{
        //    return new SourceLocation(start, length);
        //}

        private void ReportError(ErrorCode error, int start, int length, params object[] args)
        {
        }

        private void ScannerErrorHandler(int offset, int length, ErrorCode error, params object[] args)
        {
        }

        private TNode WithXamlParseError<TNode>(TNode node, ErrorCode code) where TNode : XamlSyntaxNode
        {
            return WithAdditionalDiagnostics(node, new SyntaxDiagnosticInfo(0, node.Width, code));
        }

        private TNode WithXamlParseError<TNode>(TNode node, ErrorCode code, params string[] args) where TNode : XamlSyntaxNode
        {
            return WithAdditionalDiagnostics(node, new SyntaxDiagnosticInfo(0, node.Width, code, args));
        }

        private SyntaxToken WithXamlParseError(SyntaxToken node, ErrorCode code, params string[] args)
        {
            return WithAdditionalDiagnostics(node, new SyntaxDiagnosticInfo(0, node.Width, code, args));
        }

        private void IncreaseAndCheckDepth()
        {
            _recursionDepth++;
            if (_recursionDepth > MaxDepth)
            {
                _recursionDepth = 0;
                ReportError(ErrorCode.MaxDepthReached, _scanner.StartPos, _scanner.EndPos);
                throw new InvalidOperationException(nameof(ErrorCode.MaxDepthReached));
            }
        }

        private void DecreaseDepth()
        {
            _recursionDepth--;
        }

        private void ResetDepth()
        {
            _recursionDepth = 0;
        }

        private void ValidateDepth()
        {
            Validate.Retail(_recursionDepth == 0);
        }

        #region SyntaxKind sets
        private static readonly TokenSet EndOfFile;
        private static readonly TokenSet EndOfFileOrXmlContent;
        private static readonly TokenSet XmlContent;

        // Suppress bogus FxCop message
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static LanguageParser()
        {
            EndOfFile = new TokenSet();
            EndOfFile |= SyntaxKind.EndOfFile;

            XmlContent = new TokenSet();
            XmlContent |= SyntaxKind.CharacterData;
            XmlContent |= SyntaxKind.Comment;
            XmlContent |= SyntaxKind.StartOfRegion;
            XmlContent |= SyntaxKind.EndOfRegion;
            XmlContent |= SyntaxKind.LiteralContentString;
            XmlContent |= SyntaxKind.ProcessingInstruction;
            XmlContent |= SyntaxKind.StartOfTag;

            EndOfFileOrXmlContent = new TokenSet();
            EndOfFileOrXmlContent |= SyntaxKind.EndOfFile;
            EndOfFileOrXmlContent |= LanguageParser.XmlContent;
        }
        #endregion
    }
}
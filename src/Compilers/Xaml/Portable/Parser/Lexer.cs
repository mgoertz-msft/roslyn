// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{

    interface IScannerSource
    {
        int Length { get; }
        string Read(int location, int length);
        bool IsAlreadyAString { get; }
    }

    ///<summary>The scanner state governs the behavior of GetNextToken.</summary>
    internal enum State
    {
        ///<summary>Scanning the body of an XML element. Not inside a tag. </summary>
        XML,
        ///<summary>Scanning inside a tag. Not expecting free form text. </summary>
        Tag,
        ///<summary>Scanning a non XML token.</summary>
        Other
    };

    /// <summary>
    /// Scans Xaml source text for tokens.
    /// </summary>
    internal class Lexer : AbstractLexer
    {
        private const int TriviaListInitialCapacity = 8;

        // Maximum size of tokens/trivia that we cache and use in quick scanner.
        // From what I see in our own codebase, tokens longer then 40-50 chars are 
        // not very common. 
        // So it seems reasonable to limit the sizes to some round number like 42.
        private const int MaxCachedTokenSize = 42;

        ///<summary>The position (index) of the first character making up the token.</summary>
        internal int _startPos;

        ///<summary>One more than the last position that contains a character making up the token</summary>
        internal int _endPos;

        ///<summary>One more than the last position that contains a source character</summary>
        internal int _maxPos;

        /// <summary>A running line count</summary>
        internal int _line;

        ///<summary>The source text being scanned.</summary>
        private string _text;

        ///<summary>The source text being scanned.</summary>
        private SourceText _source;

        ///<summary>The XAML parse options.</summary>
        private XamlParseOptions _options;

        /// <summary>
        /// True if the string literal was terminated with quotes.
        /// </summary>
        private bool _wasStringLiteralTerminatedProperly;

        ///<summary>True if Xml body text is all white space. Used by GetStringLiteral to decide what kind of literal to return.</summary>
        private bool _isWhitespace;

        /// <summary>True if the current token was preceeded by whitespace that wasn't reported via a whitespace token</summary>
        private bool _wasWhitespaceSkipped;

        ///<summary>True if scanning one line at a time and the current token spans over the end of the current line.</summary>
        private bool _stillInsideMultiLineToken;

        ///<summary>The last scanned string or Xml body text, with escape sequences replaced with the corresponding characters.</summary>
        private StringBuilder _stringBuilder;

        ///<summary>The last scanned identifier, with escape sequences replaced with the corresponding characters.
        ///Empty if no escape sequences were found in the last scanned identifier.</summary>
        private StringBuilder _identifierBuilder;

        ///<summary>One more than the last source position containing a character that has been appended to the identifierBuilder.
        ///This is only meaningful while an identifier is being scanned. 
        ///If equal to this.startPos, then no escape sequences have been encountered and the builder is empty and not to be used.</summary>
        private int _lastEndPosOnIdBuilder;
        //^ invariant this.startPos <= this.lastEndPosOnIdBuilder && this.lastEndPosOnIdBuilder <= this.endPos;
        //Note that (this.lastEndPosOnIdBuilder-this.startPos) is usually different from the length of the identifierBuilder, 
        //since it includes the length of the escape sequences.

        /// <summary>
        /// We share these string builders.  When a scanner is created, it grabs these builders and then nulls
        /// them.  When Done is called, the builders are returned back to the statics and can be reused.
        /// </summary>
        private static StringBuilder _sharedStringBuilder;
        private static StringBuilder _sharedIdentiferBuilder;

        ///<summary>The scanner state governs the behavior of GetNextToken. It is also used to decide on appropriate colors for tokens.</summary>
        private State _state = State.XML;

        private int _scannerState;

        private SyntaxListBuilder _leadingTriviaCache = new SyntaxListBuilder(10);
        private SyntaxListBuilder _trailingTriviaCache = new SyntaxListBuilder(10);

        private readonly LexerCache _cache;

        internal Lexer(SourceText text, XamlParseOptions options) : base(text)
        {
            _source = text;
            _options = options;

            // Try to reuse existing builders.
            _stringBuilder = System.Threading.Interlocked.Exchange(ref _sharedStringBuilder, null);
            _identifierBuilder = System.Threading.Interlocked.Exchange(ref _sharedIdentiferBuilder, null);

            // If there is another scanner currently using our shared resource, simply new another
            if (_stringBuilder == null) _stringBuilder = new StringBuilder(4096);
            if (_identifierBuilder == null) _identifierBuilder = new StringBuilder(128);

            _cache = new LexerCache();
        }

        public override void Dispose()
        {
            _cache.Free();

            base.Dispose();
        }

        public SourceText Text => this._source;

        public void Reset(int position)
        {
            this.TextWindow.Reset(position);
        }

        public bool StringLiteralTerminatedProperly { get { return _wasStringLiteralTerminatedProperly; } }

        private bool _treatNumericAsStartingChar;
        public bool IsNumericAsStartingChar { get { return _treatNumericAsStartingChar; } set { _treatNumericAsStartingChar = value; } }
        public bool IsWhitespaceOrEolBeforeStartingChar
        {
            get
            {
                int position = _startPos - 1;
                if (position >= 0 && position < _maxPos)
                    return IsWhitespaceOrEol(_text[position]);
                return false;
            }
        }

        public bool WasWhitespaceSkipped { get { return _wasWhitespaceSkipped; } }
        public bool Whitespace { get { return _isWhitespace; } }
        public int StartPos { get { return _startPos; } }
        public int EndPos { get { return _endPos; } }
        public int Line { get { return _line; } }

        /// <summary>
        /// Get a character to return 0 if the character is out of bounds.
        /// This is crafted to be inline-able by the JIT and NGEN. Ensure
        /// it is still inline-able if modified. 
        /// </summary>
        private char GetChar(int position, int maxPos)
        {
            return (position >= maxPos) ? (char)0 : _text[position];
        }

        private static int GetFullWidth(SyntaxListBuilder builder)
        {
            int width = 0;

            if (builder != null)
            {
                for (int i = 0; i < builder.Count; i++)
                {
                    width += builder[i].FullWidth;
                }
            }

            return width;
        }

        internal SyntaxToken Lex()
        {
            _leadingTriviaCache.Clear();
            this.LexSyntaxTrivia(afterFirstToken: TextWindow.Position > 0, isTrailing: false, triviaList: ref _leadingTriviaCache);
            var leading = _leadingTriviaCache;

            var tokenInfo = default(TokenInfo);

            this.Start();
            this.ScanNextToken(ref tokenInfo);
            var errors = this.GetErrors(GetFullWidth(leading));

            _trailingTriviaCache.Clear();
            this.LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, triviaList: ref _trailingTriviaCache);
            var trailing = _trailingTriviaCache;

            return Create(ref tokenInfo, leading, trailing, errors);
        }

        internal SyntaxTriviaList LexSyntaxLeadingTrivia()
        {
            _leadingTriviaCache.Clear();
            this.LexSyntaxTrivia(afterFirstToken: TextWindow.Position > 0, isTrailing: false, triviaList: ref _leadingTriviaCache);
            return new SyntaxTriviaList(default(Microsoft.CodeAnalysis.SyntaxToken),
                _leadingTriviaCache.ToListNode(), position: 0, index: 0);
        }

        internal SyntaxTriviaList LexSyntaxTrailingTrivia()
        {
            _trailingTriviaCache.Clear();
            this.LexSyntaxTrivia(afterFirstToken: true, isTrailing: true, triviaList: ref _trailingTriviaCache);
            return new SyntaxTriviaList(default(Microsoft.CodeAnalysis.SyntaxToken),
                _trailingTriviaCache.ToListNode(), position: 0, index: 0);
        }

        internal SyntaxToken Create(ref TokenInfo info, SyntaxListBuilder leading, SyntaxListBuilder trailing, SyntaxDiagnosticInfo[] errors)
        {
            Debug.Assert(info.Kind != SyntaxKind.IdentifierToken || info.Text != null);

            var leadingNode = leading?.ToListNode();
            var trailingNode = trailing?.ToListNode();

            SyntaxToken token = null;
            switch (info.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    token = SyntaxFactory.Identifier(leadingNode, info.Text, trailingNode);
                    break;
                case SyntaxKind.NumericLiteralToken:
                    break;
                case SyntaxKind.StringLiteralToken:
                    token = SyntaxFactory.Literal(leadingNode, info.Text, info.Kind, info.Text, trailingNode);
                    break;
                case SyntaxKind.CharacterLiteralToken:
                    token = SyntaxFactory.Literal(leadingNode, info.Text, info.Text, trailingNode);
                    break;
                case SyntaxKind.XamlTextLiteralNewLineToken:
                    token = SyntaxFactory.XamlTextNewLine(leadingNode, info.Text, info.Text, trailingNode);
                    break;
                case SyntaxKind.XamlTextLiteralToken:
                    token = SyntaxFactory.XamlTextLiteral(leadingNode, info.Text, info.Text, trailingNode);
                    break;
                case SyntaxKind.EndOfDocumentationCommentToken:
                case SyntaxKind.EndOfFileToken:
                    token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
                    break;
                case SyntaxKind.None:
                    token = SyntaxFactory.BadToken(leadingNode, info.Text, trailingNode);
                    break;
                default:
                    token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
                    break;
            }

            if (errors != null)
            {
                token = token?.WithDiagnosticsGreen(errors);
            }

            return token;
        }

        private void LexSyntaxTrivia(bool afterFirstToken, bool isTrailing, ref SyntaxListBuilder triviaList)
        {
            bool onlyWhitespaceOnLine = !isTrailing;

            while (true)
            {
                this.Start();
                char ch = TextWindow.PeekChar();
                if (ch == ' ')
                {
                    this.AddTrivia(this.ScanWhitespace(), ref triviaList);
                    continue;
                }
                else if (ch > 127)
                {
                    if (SyntaxFacts.IsWhitespace(ch))
                    {
                        ch = ' ';
                    }
                    else if (SyntaxFacts.IsNewLine(ch))
                    {
                        ch = '\n';
                    }
                }

                switch (ch)
                {
                    case ' ':
                    case '\t':       // Horizontal tab
                    case '\v':       // Vertical Tab
                    case '\f':       // Form-feed
                    case '\u001A':
                        this.AddTrivia(this.ScanWhitespace(), ref triviaList);
                        break;
                    case '\r':
                    case '\n':
                        this.AddTrivia(this.ScanEndOfLine(), ref triviaList);
                        if (isTrailing)
                        {
                            return;
                        }

                        onlyWhitespaceOnLine = true;
                        break;
                    default:
                        return;
                }
            }
        }

        private void AddTrivia(XamlSyntaxNode trivia, ref SyntaxListBuilder list)
        {
            if (this.HasErrors)
            {
                trivia = trivia.WithDiagnosticsGreen(this.GetErrors(leadingTriviaWidth: 0));
            }

            if (list == null)
            {
                list = new SyntaxListBuilder(TriviaListInitialCapacity);
            }

            list.Add(trivia);
        }

        /// <summary>
        /// Scans a new-line sequence (either a single new-line character or a CR-LF combo).
        /// </summary>
        /// <returns>A trivia node with the new-line text</returns>
        private XamlSyntaxNode ScanEndOfLine()
        {
            char ch;
            switch (ch = TextWindow.PeekChar())
            {
                case '\r':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '\n')
                    {
                        TextWindow.AdvanceChar();
                        return SyntaxFactory.CarriageReturnLineFeed;
                    }

                    return SyntaxFactory.CarriageReturn;
                case '\n':
                    TextWindow.AdvanceChar();
                    return SyntaxFactory.LineFeed;
                default:
                    if (SyntaxFacts.IsNewLine(ch))
                    {
                        TextWindow.AdvanceChar();
                        return SyntaxFactory.EndOfLine(ch.ToString());
                    }

                    return null;
            }
        }

        /// <summary>
        /// Scans all of the whitespace (not new-lines) into a trivia node until it runs out.
        /// </summary>
        /// <returns>A trivia node with the whitespace text</returns>
        private SyntaxTrivia ScanWhitespace()
        {
            if (_createWhitespaceTriviaFunction == null)
            {
                _createWhitespaceTriviaFunction = this.CreateWhitespaceTrivia;
            }

            int hashCode = Hash.FnvOffsetBias;  // FNV base
            bool onlySpaces = true;

        top:
            char ch = TextWindow.PeekChar();

            switch (ch)
            {
                case '\t':       // Horizontal tab
                case '\v':       // Vertical Tab
                case '\f':       // Form-feed
                case '\u001A':
                    onlySpaces = false;
                    goto case ' ';

                case ' ':
                    TextWindow.AdvanceChar();
                    hashCode = Hash.CombineFNVHash(hashCode, ch);
                    goto top;

                case '\r':      // Carriage Return
                case '\n':      // Line-feed
                    break;

                default:
                    if (ch > 127 && SyntaxFacts.IsWhitespace(ch))
                    {
                        goto case '\t';
                    }

                    break;
            }

            if (TextWindow.Width == 1 && onlySpaces)
            {
                return SyntaxFactory.Space;
            }
            else
            {
                var width = TextWindow.Width;

                if (width < MaxCachedTokenSize)
                {
                    return _cache.LookupTrivia(
                        TextWindow.CharacterWindow,
                        TextWindow.LexemeRelativeStart,
                        width,
                        hashCode,
                        _createWhitespaceTriviaFunction);
                }
                else
                {
                    return _createWhitespaceTriviaFunction();
                }
            }
        }

        private Func<SyntaxTrivia> _createWhitespaceTriviaFunction;

        private SyntaxTrivia CreateWhitespaceTrivia()
        {
            return SyntaxFactory.Whitespace(TextWindow.GetText(intern: true));
        }

        ///<summary>Returns the next token after skipping over leading whitespace.</summary>
        internal TokenInfo GetNextToken()
        {
            return this.GetNextToken(stopAtEndOfLine: false);
        }

        ///<summary>Returns the source characters corresponding to the last scanned token.</summary>
        internal string GetTokenSource()
        {
            return _text.Substring(_startPos, _endPos - _startPos);
        }

        /// <summary>Returns the string value without quotes if they are present. </summary>
        internal string GetStringLiteral()
        {
            return _stringBuilder.ToString();
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal void ScanNextToken(ref TokenInfo info)
        {
            SyntaxKind multiLineToken = (SyntaxKind)(_scannerState >> 2);
            _scannerState = 0;
            _startPos = 0;
            _stillInsideMultiLineToken = false;
            switch (multiLineToken)
            {
                case SyntaxKind.LiteralContentString:
                    _endPos = ScanXmlText(_endPos, true);
                    info.Kind = SyntaxKind.LiteralContentString;
                    info.Text = _stringBuilder.ToString();
                    break;

                case SyntaxKind.StringLiteral:
                    char quoteChar = (char)((_scannerState) >> (2 + 5));
                    _endPos = ScanXmlString(quoteChar, _endPos, true);
                    info.Kind = SyntaxKind.StringLiteral;
                    info.Text = _stringBuilder.ToString();
                    break;

                case SyntaxKind.ProcessingInstruction:
                    _endPos = ScanXmlProcessingInstructionsTag(_endPos, true);
                    info.Kind = SyntaxKind.ProcessingInstruction;
                    info.Text = _stringBuilder.ToString();
                    break;

                case SyntaxKind.Comment:
                    _endPos = ScanXmlComment(_endPos, true);
                    info.Kind = SyntaxKind.Comment;
                    info.Text = _stringBuilder.ToString();
                    break;

                default: info = GetNextToken(true); break;
            }
            if (_stillInsideMultiLineToken)
            {
                _scannerState |= ((int)info.Kind << 2) | ((int)_state);
            }
            else
                _scannerState = (int)_state;
        }

        internal int ScannerState
        {
            get { return _scannerState; }
            set
            {
                _scannerState = value;
                _state = (State)(value & 0x3);
            }
        }

        ///<summary>Returns the next token after skipping over leading whitespace.
        ///Provides for line at a time scanning.</summary>
        ///<param name="stopAtEndOfLine">If true, multi-line tokens are terminated at the first end of line.</param>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")] // its a scanner for goodness sake!
        private TokenInfo GetNextToken(bool stopAtEndOfLine)
        {
            _stringBuilder.Length = 0;
            int current = _startPos = _endPos;
            int maxPos = _maxPos;
            if (_state == State.XML)
            {
                if (current >= maxPos)
                {
                    if (!ReadSource(current))
                        return new TokenInfo { Kind = SyntaxKind.EndOfFile };
                    current = _endPos;
                    maxPos = _maxPos;
                    if (maxPos == 0)
                        return new TokenInfo { Kind = SyntaxKind.EndOfFile };
                }
                // current is not valid here, it doesn't have to be because we are calling ScanXmlText().
                Validate.Debug(_endPos < _maxPos);
                if (_text[current] == '<')
                    _state = State.Tag;
                else
                {
                    current = ScanXmlText(current, stopAtEndOfLine);
                    Validate.Debug(_startPos < current || GetChar(current, maxPos) == '<' || stopAtEndOfLine && IsEndOfLine(GetChar(current, maxPos)));
                    if (_startPos < current)
                    {
                        _endPos = current;
                        return new TokenInfo { Kind = SyntaxKind.LiteralContentString, Text = _stringBuilder.ToString() };
                    }
                    if (this.GetChar(current, maxPos) == '<')
                        this._state = State.Tag;
                    else
                    {
                        Validate.Debug(stopAtEndOfLine && IsEndOfLine(GetChar(current, maxPos)));
                        _endPos = current;
                        return new TokenInfo { Kind = SyntaxKind.EndOfFile };
                    }
                }
            }
            Validate.Debug(_endPos == current, "current was not updated correctly transitioning out of XML state");
            Validate.Debug(_startPos == _endPos, "_startPos was not updated correctly transitioning out of XML state");
            Validate.Debug(_state == State.Tag, "Should be in Tag state now, XML state didn't update the state correctly");
            Validate.Debug(_maxPos == maxPos, "maxPos was not updated correctly by XML state");

            _isWhitespace = false; //Inside a tag, or another structured area, GetStringLiteral must never return a Whitespace literal.
            _wasWhitespaceSkipped = false;

            nextToken:
            char c = (char)0;

            // Skip whitespace and update c to be the first non-whitespace char.
            for (; ;)
            {
                if (current >= maxPos)
                {
                    if (!ReadSource(current))
                        return new TokenInfo { Kind = SyntaxKind.EndOfFile };
                    current = _endPos;
                    maxPos = _maxPos;
                    Validate.Debug(current <= maxPos, "ReadSource didn't update _endPos and _maxPos correctly.");
                    Validate.Debug(maxPos <= _text.Length, "ReadSource didn't update _maxPos correctly.");
                }
                c = _text[current++];
                if (c == '\n') _line++;
                if (!IsWhitespace(c))
                    break;
                _wasWhitespaceSkipped = true;
            }
            Validate.Debug(current <= maxPos, "Skiping whitespace doesn't handle EOF correctly");

            _startPos = current - 1;
            _identifierBuilder.Length = 0;
            _lastEndPosOnIdBuilder = current - 1;

            Validate.Debug(!IsWhitespace(c), "Whitespace was not skipped correctly");
            Validate.Debug(c == GetChar(current - 1, maxPos), "current is not correct prior to switch");

            TokenInfo result = new TokenInfo { Kind = SyntaxKind.EndOfFile };
            switch (c)
            {
                case '\r':
                    if (GetChar(current, maxPos) == '\n')
                        current++;
                    goto case '\n';
                case '\n':
                    _line++;
                    if (stopAtEndOfLine)
                        result = new TokenInfo { Kind = SyntaxKind.EndOfLine };
                    else
                        goto nextToken;	//Skip over the end of the line as if it were WhiteSpace.
                    break;
                case '>':
                    _state = State.XML;
                    _endPos = current;
                    result = new TokenInfo { Kind = SyntaxKind.EndOfTag };
                    break;
                case '{': result = new TokenInfo { Kind = SyntaxKind.LeftBrace }; break;
                case '}': result = new TokenInfo { Kind = SyntaxKind.RightBrace }; break;
                case '[': result = new TokenInfo { Kind = SyntaxKind.LeftBracket }; break;
                case ']': result = new TokenInfo { Kind = SyntaxKind.RightBracket }; break;
                case '(': result = new TokenInfo { Kind = SyntaxKind.LeftParenthesis }; break;
                case ')': result = new TokenInfo { Kind = SyntaxKind.RightParenthesis }; break;
                case '=': result = new TokenInfo { Kind = SyntaxKind.Assign }; break;
                case ':': result = new TokenInfo { Kind = SyntaxKind.Colon }; break;
                case '.': result = new TokenInfo { Kind = SyntaxKind.Dot }; break;
                case ',': result = new TokenInfo { Kind = SyntaxKind.Comma }; break;
                case '+': result = new TokenInfo { Kind = SyntaxKind.Plus }; break;
                case '*': result = new TokenInfo { Kind = SyntaxKind.Star }; break;
                case '"':
                case '\'':
                    _wasStringLiteralTerminatedProperly = false;
                    current = ScanXmlString(c, current, stopAtEndOfLine);
                    result = new TokenInfo { Kind = SyntaxKind.StringLiteral, Text = _stringBuilder.ToString() };
                    break;
                case '/':
                    c = GetChar(current, maxPos);
                    if (c == '>')
                    {
                        current++;
                        _state = State.XML;
                        result = new TokenInfo { Kind = SyntaxKind.EndOfSimpleTag };
                    }
                    else
                        result = new TokenInfo { Kind = SyntaxKind.IllegalCharacter };
                    break;
                case '?':
                    c = GetChar(current, maxPos);
                    if (c == '>')
                    {
                        current++;
                        result = new TokenInfo { Kind = SyntaxKind.EndOfProcessingInstruction };
                    }
                    else
                        result = new TokenInfo { Kind = SyntaxKind.IllegalCharacter };
                    break;
                case '<':
                    result = new TokenInfo { Kind = SyntaxKind.StartOfTag };
                    int savedCurrent = current;
                    c = GetChar(current++, maxPos);
                    switch (c)
                    {
                        case '/': result = new TokenInfo { Kind = SyntaxKind.StartOfClosingTag }; break;
                        case '?':
                            current = ScanXmlProcessingInstructionsTag(current, stopAtEndOfLine);
                            _state = State.XML;
                            result = new TokenInfo { Kind = SyntaxKind.ProcessingInstruction };
                            break;
                        case '!':
                            c = GetChar(current++, maxPos);
                            switch (c)
                            {
                                case '-':
                                    if (GetChar(current, maxPos) == '-')
                                    {
                                        char region = GetChar(current + 1, maxPos);
                                        if (region == '#')
                                        {
                                            if (GetChar(current + 2, maxPos) == 'r' &&
                                                GetChar(current + 3, maxPos) == 'e' &&
                                                GetChar(current + 4, maxPos) == 'g' &&
                                                GetChar(current + 5, maxPos) == 'i' &&
                                                GetChar(current + 6, maxPos) == 'o' &&
                                                GetChar(current + 7, maxPos) == 'n')
                                            {
                                                result = new TokenInfo { Kind = SyntaxKind.StartOfRegion };
                                            }
                                            else if (GetChar(current + 2, maxPos) == 'e' &&
                                                GetChar(current + 3, maxPos) == 'n' &&
                                                GetChar(current + 4, maxPos) == 'd' &&
                                                GetChar(current + 5, maxPos) == 'r' &&
                                                GetChar(current + 6, maxPos) == 'e' &&
                                                GetChar(current + 7, maxPos) == 'g' &&
                                                GetChar(current + 8, maxPos) == 'i' &&
                                                GetChar(current + 9, maxPos) == 'o' &&
                                                GetChar(current + 10, maxPos) == 'n')
                                            {
                                                result = new TokenInfo { Kind = SyntaxKind.EndOfRegion };
                                            }
                                        }
                                        else
                                        {
                                            result = new TokenInfo { Kind = SyntaxKind.Comment };
                                        }
                                        _state = State.XML;
                                        current = ScanXmlComment(current + 1, stopAtEndOfLine);
                                    }
                                    else
                                        current = savedCurrent;
                                    break;
                                case '[':
                                    if (GetChar(current++, maxPos) == 'C' &&
                                      GetChar(current++, maxPos) == 'D' &&
                                      GetChar(current++, maxPos) == 'A' &&
                                      GetChar(current++, maxPos) == 'T' &&
                                      GetChar(current++, maxPos) == 'A' &&
                                      GetChar(current++, maxPos) == '[')
                                    {
                                        current = ScanXmlCharacterData(current, stopAtEndOfLine);
                                        _state = State.XML;
                                        result = new TokenInfo { Kind = SyntaxKind.CharacterData };
                                    }
                                    else
                                    {
                                        current = savedCurrent;
                                    }
                                    break;
                                default:
                                    current = savedCurrent;
                                    break;
                            }
                            break;
                        default:
                            current = savedCurrent;
                            break;
                    }
                    break;
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    if (IsNumericAsStartingChar)
                        goto case 'A';
                    else
                        goto default;

                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case '_':
                    while (true)
                    {
                        // We don't have to check for current == maxPos here because we are guarenteed
                        // that we are not on a whitespace and maxPos will only occur after a whitespace.
                        switch ((c = GetChar(current++, maxPos)))
                        {
                            case 'A':
                            case 'B':
                            case 'C':
                            case 'D':
                            case 'E':
                            case 'F':
                            case 'G':
                            case 'H':
                            case 'I':
                            case 'J':
                            case 'K':
                            case 'L':
                            case 'M':
                            case 'N':
                            case 'O':
                            case 'P':
                            case 'Q':
                            case 'R':
                            case 'S':
                            case 'T':
                            case 'U':
                            case 'V':
                            case 'W':
                            case 'X':
                            case 'Y':
                            case 'Z':
                            case 'a':
                            case 'b':
                            case 'c':
                            case 'd':
                            case 'e':
                            case 'f':
                            case 'g':
                            case 'h':
                            case 'i':
                            case 'j':
                            case 'k':
                            case 'l':
                            case 'm':
                            case 'n':
                            case 'o':
                            case 'p':
                            case 'q':
                            case 'r':
                            case 's':
                            case 't':
                            case 'u':
                            case 'v':
                            case 'w':
                            case 'x':
                            case 'y':
                            case 'z':
                            case '0':
                            case '1':
                            case '2':
                            case '3':
                            case '4':
                            case '5':
                            case '6':
                            case '7':
                            case '8':
                            case '9':
                            case '_':
                                continue;
                            case '.':
                                if (_state == State.Tag)
                                    continue;
                                else
                                    goto default;
                            default:
                                int lastCurrent = current - 1;
                                bool escape = false;
                                string cAsString = c.ToString();
                                if (c == '&')
                                {
                                    current = ScanXmlEscapedChar(current, ref cAsString);
                                    escape = true;
                                }
                                else if (c <= 128)
                                {
                                    current--;
                                    break;
                                }

                                if (IsIdentifierPartChar(cAsString, _state == State.Tag))
                                {
                                    if (escape)
                                    {
                                        _identifierBuilder.Append(GetSubstring(_lastEndPosOnIdBuilder, lastCurrent - _lastEndPosOnIdBuilder));
                                        _identifierBuilder.Append(cAsString);
                                        _lastEndPosOnIdBuilder = current;
                                    }
                                    continue;
                                }
                                current--;
                                break;
                        }
                        break;
                    }
                    result = new TokenInfo { Kind = SyntaxKind.Identifier, Text = GetIdentifierString(_startPos, current)};
                    break;

                default:
                    {
                        bool escape = false;
                        string cAsString = c.ToString();
                        if (c == '&')
                        {
                            current = ScanXmlEscapedChar(current, ref cAsString);
                            escape = true;
                        }
                        else if (c <= 128)
                        {
                            result = new TokenInfo { Kind = SyntaxKind.IllegalCharacter };
                            break;
                        }

                        if (IsIdentifierStartChar(cAsString))
                        {
                            if (escape)
                            {
                                _identifierBuilder.Append(cAsString);
                                _lastEndPosOnIdBuilder = current;
                            }
                            goto case 'A';
                        }
                        else
                            result = new TokenInfo { Kind = SyntaxKind.IllegalCharacter };
                        break;
                    }
            }
            Validate.Debug(result.Kind != SyntaxKind.EndOfFile, "result was not updated correctly");
            _endPos = current;
            return result;
        }

        private string GetIdentifierString(int start, int end)
        {
            if (_identifierBuilder != null && _lastEndPosOnIdBuilder > start)
            {
                Validate.Retail(_lastEndPosOnIdBuilder == end);
                return _identifierBuilder.ToString();
            }
            //Get here if the identifier had no escape sequences.
            //Just get a substring from the source text.
            return _text.Substring(start, end - start);
        }

        ///<summary>Retrieves a substring of the source text. The substring starts at a specified character position and has a specified length.</summary>
        private string GetSubstring(int start, int length)
        {
            return _text.Substring(start, length);
        }

        private int ScanXmlString(char closingQuote, int current, bool stopAtEndOfLine)
        {
            int maxPos = _maxPos;
            int startLine = _line;
            for (; ;)
            {
                if (current >= maxPos)
                {
                    bool more = ReadSource(current);
                    current = _endPos;
                    maxPos = _maxPos;
                    if (!more)
                    {
                        HandleError(current, ErrorCode.UnterminatedString, closingQuote.ToString()); //EOF encountered before closing quote
                                                                                                 //TODO: find a likely point where the closing quote should be. For example a > or a space followed by an identifier followed by =.
                        break;
                    }
                }
                char c = _text[current++];
                if (c == closingQuote)
                {
                    _wasStringLiteralTerminatedProperly = true;
                    break;
                }
                if (c == '\n')
                    _line++;
                if (stopAtEndOfLine && IsEndOfLine(c))
                {
                    _stillInsideMultiLineToken = true;
                    break;
                }
                if (c == '<')
                {
                    if (_line != startLine)
                    {
                        // Assume they just forgot to close the quote
                        HandleError(current - 1, ErrorCode.UnterminatedString, closingQuote.ToString()); //EOF encountered before closing quote
                        return current - 1;
                    }
                    else
                        HandleError(current, ErrorCode.UnescapedOpenTag); //unescaped < characters are not allowed inside xml strings.
                }
                if (c == '&')
                {
                    string xmlChar = "";
                    current = ScanXmlEscapedChar(current, ref xmlChar);
                    _stringBuilder.Append(xmlChar);
                }
                else
                    _stringBuilder.Append(c);
            }
            return current;
        }

        private int ScanXmlCharacterData(int current, bool stopAtEndOfLine)
        {
            int maxPos = _maxPos;
            string text = _text;
            for (; ;)
            {
                if (current >= maxPos)
                {
                    bool more = ReadSource(current);
                    current = _endPos;
                    maxPos = _maxPos;
                    text = _text;
                    if (!more)
                    {
                        HandleError(current, ErrorCode.UnterminatedCharacterData);
                        //TODO: try to find a likely point where the terminator should be. For example the start of a closing tag or ]> or > ws <
                        break;
                    }
                }
                char c = text[current++];
                if (c == ']')
                {
                    c = GetChar(current, maxPos);
                    if (c == ']')
                    {
                        c = GetChar(current + 1, maxPos);
                        if (c == '>')
                        {
                            current += 2;
                            break;
                        }
                    }
                }
                if (c == '\n')
                    _line++;
                if (stopAtEndOfLine && IsEndOfLine(c))
                {
                    _stillInsideMultiLineToken = true;
                    break;
                }
                _stringBuilder.Append(c);
            }
            return current;
        }

        internal int ScanXmlComment(int current, bool stopAtEndOfLine)
        {
            int maxPos = _maxPos;
            string text = _text;
            for (; ;)
            {
                if (current >= maxPos)
                {
                    bool more = ReadSource(current);
                    current = _endPos;
                    maxPos = _maxPos;
                    text = _text;
                    if (!more)
                    {
                        if (stopAtEndOfLine)
                            _stillInsideMultiLineToken = true;
                        else
                            HandleError(current, ErrorCode.UnterminatedComment);
                        //TODO: try to find a likely point where the terminator should be. For example < or > or -> or --!>.
                        break;
                    }
                }
                char c = text[current++];
                if (c == '-')
                {
                    c = GetChar(current, maxPos);
                    if (c == '-')
                    {
                        c = GetChar(current + 1, maxPos);
                        if (c == '>')
                        {
                            current += 2;
                            break;
                        }
                        else
                        {
                            if (c == '!' && GetChar(current + 2, maxPos) == '>')
                            {
                                current += 3;
                                HandleError(current, ErrorCode.CommentedEndedWithDoubleHyphenBangGreaterThan);
                                break;
                            }
                            current++;
                            HandleError(current, ErrorCode.CommentWithDoubleHyphen);
                        }
                    }
                }
                if (c == '\n')
                    _line++;
                if (stopAtEndOfLine && IsEndOfLine(c))
                {
                    _stillInsideMultiLineToken = true;
                    break;
                }
            }
            return current;
        }

        ///<summary>Scans an escape sequence and returns the corresponding character.
        ///Advances this.endPos to the character following the escape sequence. Reports any errors. </summary>
        private int ScanXmlEscapedChar(int current, ref string s)
        {
            int maxPos = _maxPos;
            char c = GetChar(current, maxPos);
            if (c == '#')
                return ScanNumericCharEntity(current, ref s);

            //Look for a named entity (e.g. &amp; &lt; &gt; &quot; &apos;)
            int start = current;
            for (; c != 0 && c != ';' && c != '"' && c != '\'' && c != '>' && c != '<' && !IsWhitespaceOrEol(c); c = GetChar(++current, maxPos))
                ;
            if (c == ';')
            {
                string name = this.GetSubstring(start, current - start);
                switch (name)
                {
                    case "amp":
                        c = '&';
                        break;
                    case "lt":
                        c = '<';
                        break;
                    case "gt":
                        c = '>';
                        break;
                    case "quot":
                        c = '"';
                        break;
                    case "apos":
                        c = '\'';
                        break;
                    default:
                        // Use current + 1 to have error underline terminating ';'. Consumption of ';' handled elsewhere. 
                        // Manage start of entity to avoid from-start-of-text errors when entity is in a string. start -1 to include '&'.
                        HandleError(start - 1, current - start + 1, ErrorCode.NoSuchNamedEntity, name);
                        c = '_';
                        break;
                }
                current++; // consume ';'
            }
            else
            {
                HandleError(current, ErrorCode.MissingSemicolonInEntity);
                c = (char)0;
            }
            s = c.ToString();
            return current;
        }

        private int ScanNumericCharEntity(int current, ref string s)
        {
            int start = current;
            int maxPos = _maxPos;
            char c = GetChar(++current, maxPos);
            int val;
            if (c == 'x')
                // Use current + 1 as first char is passed in parameter
                current = ScanHexCharValue(current + 1, GetChar(current + 1, maxPos), start, out val);
            else
                current = ScanDecimalCharValue(current, c, start, out val);
            if (val < 0)
                s = "_"; //error encountered and reported
            else
                s = UnicodeUtilities.IntToUtf16String(val);
            return current + 1;	// consume ';'
        }

        internal int ScanDecimalCharValue(int current, char ch, int start, out int val)
        {
            int result = 0;
            int maxPos = _maxPos;
            bool overflow = false;
            for (; ch != 0 && ch != ';'; ch = GetChar(++current, maxPos))
            {
                if (!overflow) // allow current pos to advance to terminator in overflow case, to underline whole entity
                {
                    int d = 0;
                    if (ch >= '0' && ch <= '9')
                    {
                        d = (int)(ch - '0');
                    }
                    else
                    {
                        HandleError(current, 1, ErrorCode.BadDecimalDigit, GetSubstring(current, 1));
                        result = -1;
                        break;
                    }
                    if (result > ((int.MaxValue - d) / 10))
                    {
                        // Rather than breaking the loop here, allow loop to continue so that error reporting
                        // can underline the whole entity
                        overflow = true;
                    }
                    else
                    {
                        result = (result * 10) + d;
                    }
                }
            }
            if (result != -1)
            {
                if (ch != ';')
                {
                    HandleError(current, ErrorCode.ExpectedDifferentToken, ";");
                    result = -1;
                }
                else if (overflow || !UnicodeUtilities.IsValidUnicodeValue(result))
                {
                    // Manage start of entity to avoid from-start-of-text errors when entity is in a string. start -1 to include '&'.
                    // Length extended by 1 (i.e. + 2) to include trailing ';' in error.
                    HandleError(start - 1, current - start + 2, ErrorCode.EntityOverflow, this.GetSubstring(start, current - start));
                    result = -1;
                }
            }
            val = result;
            return current;
        }

        internal int ScanHexCharValue(int current, char ch, int start, out int val)
        {
            int result = 0;
            int maxPos = _maxPos;
            bool overflow = false;
            for (; ch != 0 && ch != ';' && ch != '"' && ch != '\'' && ch != '>' && ch != '<'; ch = GetChar(++current, maxPos))
            {
                if (!overflow) // allow current pos to advance to terminator in overflow case, to underline whole entity
                {
                    int d = 0;
                    if (ch >= '0' && ch <= '9')
                    {
                        d = (int)(ch - '0');
                    }
                    else if (ch >= 'a' && ch <= 'f')
                    {
                        d = (int)(ch - 'a') + 10;
                    }
                    else if (ch >= 'A' && ch <= 'F')
                    {
                        d = (int)(ch - 'A') + 10;
                    }
                    else
                    {
                        HandleError(current, 1, ErrorCode.BadHexDigit, this.GetSubstring(current, 1));
                        result = -1;
                        break;
                    }
                    if (result > ((int.MaxValue - d) / 16))
                    {
                        // Rather than breaking the loop here, allow loop to continue so that error reporting
                        // can underline the whole entity
                        overflow = true;
                    }
                    else
                    {
                        result = (result * 16) + d;
                    }
                }
            }
            if (result != -1)
            {
                if (ch != ';')
                {
                    HandleError(current, ErrorCode.ExpectedDifferentToken, ";");
                    result = -1;
                }
                else if (overflow || !UnicodeUtilities.IsValidUnicodeValue(result))
                {
                    // Manage start of entity to avoid from-start-of-text errors when entity is in a string. start -1 to include '&'.
                    // Length extended by 1 (i.e. + 2) to include trailing ';' in error.
                    HandleError(start - 1, current - start + 2, ErrorCode.EntityOverflow, this.GetSubstring(start, current - start));
                    result = -1;
                }
            }
            val = result;
            return current;
        }

        internal int ScanXmlProcessingInstructionsTag(int current, bool stopAtEndOfLine)
        {
            int maxPos = _maxPos;
            string text = _text;
            for (; ;)
            {
                if (current >= maxPos)
                {
                    bool more = ReadSource(current);
                    current = _endPos;
                    if (!more)
                    {
                        HandleError(current, ErrorCode.UnterminatedProcessingInstruction);
                        //TODO: try to find a likely point where the terminator should be. For example < or >.
                        break;
                    }
                }
                char c = text[current++];
                if (c == '?')
                {
                    c = GetChar(current, maxPos);
                    if (c == '>')
                    {
                        current++;
                        break;
                    }
                }
                if (c == '\n')
                    _line++;
                if (stopAtEndOfLine && IsEndOfLine(c))
                {
                    _stillInsideMultiLineToken = true;
                    break;
                }
            }
            return current;
        }

        private int ScanXmlText(int current, bool stopAtEndOfLine)
        {
            bool isWhitespace = true;
            int maxPos = _maxPos;
            string text = _text;
            for (; ;)
            {
                if (current >= maxPos)
                {
                    bool more = ReadSource(current);
                    current = _endPos;
                    maxPos = _maxPos;
                    if (!more)
                    {
                        if (!isWhitespace)
                            HandleError(current, ErrorCode.UnterminatedXmlText); //Non whitespace follows last end tag
                        break;
                    }
                }
                char ch = text[current++];
                if (ch == '<')
                {
                    current--;
                    break;
                }
                if (ch == '\n')
                    _line++;
                if (stopAtEndOfLine && IsEndOfLine(ch))
                {
                    _stillInsideMultiLineToken = true;
                    break;
                }
                if (ch == '&')
                {
                    isWhitespace = false;
                    string xmlChar = "";
                    current = ScanXmlEscapedChar(current, ref xmlChar);
                    _stringBuilder.Append(xmlChar);
                }
                else
                {
                    isWhitespace = isWhitespace && IsWhitespaceOrEol(ch);
                    _stringBuilder.Append(ch);
                }
            }
            _isWhitespace = isWhitespace;
            return current;
        }

        internal static bool IsWhitespace(char c)
        {
            switch (c)
            {
                case (char)0x09:
                case (char)0x0B:
                case (char)0x0C:
                case (char)0x20:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsWhitespaceOrEol(char c)
        {
            switch (c)
            {
                case (char)0x09:
                case (char)0x0A:
                case (char)0x0B:
                case (char)0x0C:
                case (char)0x0D:
                case (char)0x20:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsEndOfLine(char c)
        {
            return c == (char)0xD || c == (char)0xA;
        }

        internal static bool IsIdentifierPartChar(string s, bool inTag)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            if (IsIdentifierCharHelper(s, true))
                return true;
            if ('0' <= s[0] && s[0] <= '9' || s[0] == '-' || (s[0] == '.' && inTag))
                return true;
            return false;
        }

        ///<summary>If c is the first character of an escape sequence representing an identifier start character
        ///then the preceding characters as well as the unescaped character are appended to this.identifierBuilder
        ///and this.endPos and this.lastEndPosOnIdBuilder are advanced to one past the position of the last character in the escape sequence.</summary>
        internal static bool IsIdentifierStartChar(string s)
        {
            return IsIdentifierCharHelper(s, false);
        }

        internal static bool IsIdentifierCharHelper(string s, bool partChar)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            Debug.Assert(s.Length <= 2, "A Unicode character has a max of 2 chars.");

            UnicodeCategory ccat = 0;
            if ('a' <= s[0] && s[0] <= 'z' || 'A' <= s[0] && s[0] <= 'Z' || s[0] == '_')
                return true;
            if (s[0] < 128)
                return false;

            // TODO:MGoertz
            //ccat = char.GetUnicodeCategory(s, 0);
            //switch (ccat)
            //{
            //    case UnicodeCategory.LowercaseLetter: //Ll
            //    case UnicodeCategory.UppercaseLetter: //Lu
            //    case UnicodeCategory.OtherLetter: //Lo
            //    case UnicodeCategory.TitlecaseLetter: //Lt
            //    case UnicodeCategory.LetterNumber: //Nl
            //        return true;
            //    case UnicodeCategory.SpacingCombiningMark: //Mc
            //    case UnicodeCategory.EnclosingMark:	//Me
            //    case UnicodeCategory.NonSpacingMark: //Mn
            //    case UnicodeCategory.ModifierLetter: //Lm
            //    case UnicodeCategory.DecimalDigitNumber: //Nd
            //        return partChar;
            //    default:
            //        return false;
            //}
            return false;
        }

        private void HandleError(int current, ErrorCode error, params object[] messageParameters)
        {
            HandleError(current, current - _startPos, error, messageParameters);
        }

        private void HandleError(int start, int length, ErrorCode error, params object[] messageParameters)
        {
            //if (_errorHandler == null)
            //    return;
            //_errorHandler(start, length, error, messageParameters);
        }

        internal bool ReadSource(int current)
        {
            if (current == 0)
            {
                int length = _source.Length;
                char[] buffer = new char[length];
                _source.CopyTo(0, buffer, destinationIndex: 0, count: length);

                _text = new string(buffer);
                if (_text == null)
                {
                    return false;
                }
                _maxPos = _text.Length;
                if (_maxPos < 1 || !IsWhitespaceOrEol(_text[_maxPos - 1]))
                    _text = _text + " ";
                return true;
            }
            _endPos = current;
            return false;
        }
    }

    internal struct TokenInfo
    {
        // scanned values
        internal SyntaxKind Kind;
        internal string Text;
    }
}
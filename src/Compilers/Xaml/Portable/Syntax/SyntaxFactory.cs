// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Xaml.Syntax;
using Microsoft.CodeAnalysis.Text;
using InternalSyntax = Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax;
using System.Xml.Linq;
using Roslyn.Utilities;
using Microsoft.CodeAnalysis.Syntax;

namespace Microsoft.CodeAnalysis.Xaml
{
    /// <summary>
    /// A class containing factory methods for constructing syntax nodes, tokens and trivia.
    /// </summary>
    public static partial class SyntaxFactory
    {
        /// <summary>
        /// A trivia with kind EndOfLineTrivia containing both the carriage return and line feed characters.
        /// </summary>
        public static SyntaxTrivia CarriageReturnLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.CarriageReturnLineFeed;

        /// <summary>
        /// A trivia with kind EndOfLineTrivia containing a single line feed character.
        /// </summary>
        public static SyntaxTrivia LineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.LineFeed;

        /// <summary>
        /// A trivia with kind EndOfLineTrivia containing a single carriage return character.
        /// </summary>
        public static SyntaxTrivia CarriageReturn { get; } = Syntax.InternalSyntax.SyntaxFactory.CarriageReturn;

        /// <summary>
        ///  A trivia with kind WhitespaceTrivia containing a single space character.
        /// </summary>
        public static SyntaxTrivia Space { get; } = Syntax.InternalSyntax.SyntaxFactory.Space;

        /// <summary>
        /// A trivia with kind WhitespaceTrivia containing a single tab character.
        /// </summary>
        public static SyntaxTrivia Tab { get; } = Syntax.InternalSyntax.SyntaxFactory.Tab;

        /// <summary>
        /// An elastic trivia with kind EndOfLineTrivia containing both the carriage return and line feed characters.
        /// Elastic trivia are used to denote trivia that was not produced by parsing source text, and are usually not
        /// preserved during formatting.
        /// </summary>
        public static SyntaxTrivia ElasticCarriageReturnLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturnLineFeed;

        /// <summary>
        /// An elastic trivia with kind EndOfLineTrivia containing a single line feed character. Elastic trivia are used
        /// to denote trivia that was not produced by parsing source text, and are usually not preserved during
        /// formatting.
        /// </summary>
        public static SyntaxTrivia ElasticLineFeed { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticLineFeed;

        /// <summary>
        /// An elastic trivia with kind EndOfLineTrivia containing a single carriage return character. Elastic trivia
        /// are used to denote trivia that was not produced by parsing source text, and are usually not preserved during
        /// formatting.
        /// </summary>
        public static SyntaxTrivia ElasticCarriageReturn { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticCarriageReturn;

        /// <summary>
        /// An elastic trivia with kind WhitespaceTrivia containing a single space character. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        public static SyntaxTrivia ElasticSpace { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticSpace;

        /// <summary>
        /// An elastic trivia with kind WhitespaceTrivia containing a single tab character. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        public static SyntaxTrivia ElasticTab { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticTab;

        /// <summary>
        /// An elastic trivia with kind WhitespaceTrivia containing no characters. Elastic marker trivia are included
        /// automatically by factory methods when trivia is not specified. Syntax formatting will replace elastic
        /// markers with appropriate trivia.
        /// </summary>
        public static SyntaxTrivia ElasticMarker { get; } = Syntax.InternalSyntax.SyntaxFactory.ElasticZeroSpace;

        /// <summary>
        /// Creates a trivia with kind EndOfLineTrivia containing the specified text. 
        /// </summary>
        /// <param name="text">The text of the end of line. Any text can be specified here, however only carriage return and
        /// line feed characters are recognized by the parser as end of line.</param>
        public static SyntaxTrivia EndOfLine(string text)
        {
            return Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: false);
        }

        /// <summary>
        /// Creates a trivia with kind EndOfLineTrivia containing the specified text. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        /// <param name="text">The text of the end of line. Any text can be specified here, however only carriage return and
        /// line feed characters are recognized by the parser as end of line.</param>
        public static SyntaxTrivia ElasticEndOfLine(string text)
        {
            return Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: true);
        }

        [Obsolete("Use SyntaxFactory.EndOfLine or SyntaxFactory.ElasticEndOfLine")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static SyntaxTrivia EndOfLine(string text, bool elastic)
        {
            return Syntax.InternalSyntax.SyntaxFactory.EndOfLine(text, elastic);
        }

        /// <summary>
        /// Creates a trivia with kind WhitespaceTrivia containing the specified text.
        /// </summary>
        /// <param name="text">The text of the whitespace. Any text can be specified here, however only specific
        /// whitespace characters are recognized by the parser.</param>
        public static SyntaxTrivia Whitespace(string text)
        {
            return Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);
        }

        /// <summary>
        /// Creates a trivia with kind WhitespaceTrivia containing the specified text. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        /// <param name="text">The text of the whitespace. Any text can be specified here, however only specific
        /// whitespace characters are recognized by the parser.</param>
        public static SyntaxTrivia ElasticWhitespace(string text)
        {
            return Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);
        }

        [Obsolete("Use SyntaxFactory.Whitespace or SyntaxFactory.ElasticWhitespace")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static SyntaxTrivia Whitespace(string text, bool elastic)
        {
            return Syntax.InternalSyntax.SyntaxFactory.Whitespace(text, elastic);
        }

        /// <summary>
        /// Creates a trivia with kind either SingleLineCommentTrivia or MultiLineCommentTrivia containing the specified
        /// text.
        /// </summary>
        /// <param name="text">The entire text of the comment including the leading '//' token for single line comments
        /// or stop or start tokens for multiline comments.</param>
        public static SyntaxTrivia Comment(string text)
        {
            return Syntax.InternalSyntax.SyntaxFactory.Comment(text);
        }

        /// <summary>
        /// Trivia nodes represent parts of the program text that are not parts of the
        /// syntactic grammar, such as spaces, newlines, comments, preprocessor
        /// directives, and disabled code.
        /// </summary>
        /// <param name="kind">
        /// A <see cref="SyntaxKind"/> representing the specific kind of <see cref="SyntaxTrivia"/>. One of
        /// <see cref="SyntaxKind.WhitespaceTrivia"/>, <see cref="SyntaxKind.EndOfLineTrivia"/>,
        /// <see cref="SyntaxKind.SingleLineCommentTrivia"/>, <see cref="SyntaxKind.MultiLineCommentTrivia"/>,
        /// <see cref="SyntaxKind.DocumentationCommentExteriorTrivia"/>, <see cref="SyntaxKind.DisabledTextTrivia"/>
        /// </param>
        /// <param name="text">
        /// The actual text of this token.
        /// </param>
        public static SyntaxTrivia SyntaxTrivia(SyntaxKind kind, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            switch (kind)
            {
                case SyntaxKind.DisabledTextTrivia:
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                case SyntaxKind.EndOfLineTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.WhitespaceTrivia:
                    return new SyntaxTrivia(default(SyntaxToken), new Syntax.InternalSyntax.SyntaxTrivia(kind, text, null, null), 0, 0);
                default:
                    throw new ArgumentException("kind");
            }
        }

        /// <summary>
        /// Creates a token corresponding to a syntax kind. This method can be used for token syntax kinds whose text
        /// can be inferred by the kind alone.
        /// </summary>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <returns></returns>
        public static SyntaxToken Token(SyntaxKind kind)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Token(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token corresponding to syntax kind. This method can be used for token syntax kinds whose text can
        /// be inferred by the kind alone.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Token(leading.Node, kind, trailing.Node));
        }

        /// <summary>
        /// Creates a token corresponding to syntax kind. This method gives control over token Text and ValueText.
        /// 
        /// For example, consider the text '&lt;see cref="operator &amp;#43;"/&gt;'.  To create a token for the value of
        /// the operator symbol (&amp;#43;), one would call 
        /// Token(default(SyntaxTriviaList), SyntaxKind.PlusToken, "&amp;#43;", "+", default(SyntaxTriviaList)).
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="text">The text from which this token was created (e.g. lexed).</param>
        /// <param name="valueText">How XAML should interpret the text of this token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, string text, string valueText, SyntaxTriviaList trailing)
        {
            switch (kind)
            {
                case SyntaxKind.IdentifierToken:
                    // Have a different representation.
                    throw new ArgumentException(XamlResources.UseVerbatimIdentifier, nameof(kind));
                case SyntaxKind.CharacterLiteralToken:
                    // Value should not have type string.
                    throw new ArgumentException(XamlResources.UseLiteralForTokens, nameof(kind));
                case SyntaxKind.NumericLiteralToken:
                    // Value should not have type string.
                    throw new ArgumentException(XamlResources.UseLiteralForNumeric, nameof(kind));
            }

            if (!SyntaxFacts.IsAnyToken(kind))
            {
                throw new ArgumentException(string.Format(XamlResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), nameof(kind));
            }

            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Token(leading.Node, kind, text, valueText, trailing.Node));
        }

        /// <summary>
        /// Creates a missing token corresponding to syntax kind. A missing token is produced by the parser when an
        /// expected token is not found. A missing token has no text and normally has associated diagnostics.
        /// </summary>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        public static SyntaxToken MissingToken(SyntaxKind kind)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.MissingToken(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a missing token corresponding to syntax kind. A missing token is produced by the parser when an
        /// expected token is not found. A missing token has no text and normally has associated diagnostics.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken MissingToken(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.MissingToken(leading.Node, kind, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// <param name="text">The raw text of the identifier name, including any escapes or leading '@'
        /// character.</param>
        /// </summary>
        public static SyntaxToken Identifier(string text)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Identifier(ElasticMarker.UnderlyingNode, text, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the identifier name, including any escapes or leading '@'
        /// character.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Identifier(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Identifier(leading.Node, text, trailing.Node));
        }

        /// <summary>
        /// Creates a verbatim token with kind IdentifierToken containing the specified text.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the identifier name, including any escapes or leading '@'
        /// character as it is in source.</param>
        /// <param name="valueText">The canonical value of the token's text.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken VerbatimIdentifier(SyntaxTriviaList leading, string text, string valueText, SyntaxTriviaList trailing)
        {
            if (text.StartsWith("@", StringComparison.Ordinal))
            {
                throw new ArgumentException("text should not start with an @ character.");
            }

            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Identifier(SyntaxKind.IdentifierName, leading.Node, "@" + text, valueText, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="contextualKind">An alternative SyntaxKind that can be inferred for this token in special
        /// contexts. These are usually keywords.</param>
        /// <param name="text">The raw text of the identifier name, including any escapes or leading '@'
        /// character.</param>
        /// <param name="valueText">The text of the identifier name without escapes or leading '@' character.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        /// <returns></returns>
        public static SyntaxToken Identifier(SyntaxTriviaList leading, SyntaxKind contextualKind, string text, string valueText, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(InternalSyntax.SyntaxFactory.Identifier(contextualKind, leading.Node, text, valueText, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from a 4-byte signed integer value.
        /// </summary>
        /// <param name="value">The 4-byte signed integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(int value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 4-byte signed integer value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 4-byte signed integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, int value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 4-byte signed integer value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 4-byte signed integer value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, int value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from a 4-byte unsigned integer value.
        /// </summary>
        /// <param name="value">The 4-byte unsigned integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(uint value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 4-byte unsigned integer value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 4-byte unsigned integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, uint value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 4-byte unsigned integer value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 4-byte unsigned integer value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, uint value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from an 8-byte signed integer value.
        /// </summary>
        /// <param name="value">The 8-byte signed integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(long value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte signed integer value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte signed integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, long value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte signed integer value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte signed integer value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, long value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from an 8-byte unsigned integer value.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(ulong value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte unsigned integer value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte unsigned integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, ulong value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte unsigned integer value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte unsigned integer value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, ulong value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from a 4-byte floating point value.
        /// </summary>
        /// <param name="value">The 4-byte floating point value to be represented by the returned token.</param>
        public static SyntaxToken Literal(float value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 4-byte floating point value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 4-byte floating point value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, float value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 4-byte floating point value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 4-byte floating point value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, float value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from an 8-byte floating point value.
        /// </summary>
        /// <param name="value">The 8-byte floating point value to be represented by the returned token.</param>
        public static SyntaxToken Literal(double value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte floating point value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte floating point value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, double value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte floating point value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte floating point value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, double value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from a decimal value.
        /// </summary>
        /// <param name="value">The decimal value to be represented by the returned token.</param>
        public static SyntaxToken Literal(decimal value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.IncludeTypeSuffix), value);
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding decimal value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The decimal value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, decimal value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding decimal value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The decimal value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, decimal value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind StringLiteralToken from a string value.
        /// </summary>
        /// <param name="value">The string value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string value)
        {
            return Literal(SymbolDisplay.FormatLiteral(value, quote: true), value);
        }

        /// <summary>
        /// Creates a token with kind StringLiteralToken from the text and corresponding string value.
        /// </summary>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The string value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, string value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind StringLiteralToken from the text and corresponding string value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The string value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind CharacterLiteralToken from a character value.
        /// </summary>
        /// <param name="value">The character value to be represented by the returned token.</param>
        public static SyntaxToken Literal(char value)
        {
            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters), value);
        }

        /// <summary>
        /// Creates a token with kind CharacterLiteralToken from the text and corresponding character value.
        /// </summary>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The character value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, char value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a token with kind CharacterLiteralToken from the text and corresponding character value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The character value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, char value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind BadToken.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the bad token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken BadToken(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.BadToken(leading.Node, text, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind XamlTextLiteralToken.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The xml text value.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken XamlTextLiteral(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.XamlTextLiteral(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates the syntax representation of an xml text attribute.
        /// </summary>
        /// <param name="name">The name of the xml text attribute.</param>
        /// <param name="value">The value of the xml text attribute.</param>
        public static XamlTextAttributeSyntax XamlTextAttribute(string name, string value)
        {
            return XamlTextAttribute(name, XamlTextLiteral(value));
        }

        /// <summary>
        /// Creates the syntax representation of an xml text attribute.
        /// </summary>
        /// <param name="name">The name of the xml text attribute.</param>
        /// <param name="textTokens">A list of tokens used for the value of the xml text attribute.</param>
        public static XamlTextAttributeSyntax XamlTextAttribute(string name, params SyntaxToken[] textTokens)
        {
            return XamlTextAttribute(XamlName(name), SyntaxKind.DoubleQuoteToken, TokenList(textTokens));
        }

        /// <summary>
        /// Creates the syntax representation of an xml text attribute.
        /// </summary>
        /// <param name="name">The name of the xml text attribute.</param>
        /// <param name="quoteKind">The kind of the quote token to be used to quote the value (e.g. " or ').</param>
        /// <param name="textTokens">A list of tokens used for the value of the xml text attribute.</param>
        public static XamlTextAttributeSyntax XamlTextAttribute(string name, SyntaxKind quoteKind, SyntaxTokenList textTokens)
        {
            return XamlTextAttribute(XamlName(name), quoteKind, textTokens);
        }

        /// <summary>
        /// Creates the syntax representation of an xml text attribute.
        /// </summary>
        /// <param name="name">The name of the xml text attribute.</param>
        /// <param name="quoteKind">The kind of the quote token to be used to quote the value (e.g. " or ').</param>
        /// <param name="textTokens">A list of tokens used for the value of the xml text attribute.</param>
        public static XamlTextAttributeSyntax XamlTextAttribute(XamlNameSyntax name, SyntaxKind quoteKind, SyntaxTokenList textTokens)
        {
            return XamlTextAttribute(name, Token(quoteKind), textTokens, Token(quoteKind))
                .WithLeadingTrivia(Whitespace(" "));
        }

        /// <summary>
        /// Creates the syntax representation of an xml element that spans multiple text lines.
        /// </summary>
        /// <param name="localName">The name of the xml element.</param>
        /// <param name="content">A list of syntax nodes that represents the content of the xml multi line element.</param>
        public static XamlElementSyntax XamlMultiLineElement(string localName, SyntaxList<XamlSyntaxNode> content)
        {
            return XamlMultiLineElement(XamlName(localName), content);
        }

        /// <summary>
        /// Creates the syntax representation of an xml element that spans multiple text lines.
        /// </summary>
        /// <param name="name">The name of the xml element.</param>
        /// <param name="content">A list of syntax nodes that represents the content of the xml multi line element.</param>
        public static XamlElementSyntax XamlMultiLineElement(XamlNameSyntax name, SyntaxList<XamlSyntaxNode> content)
        {
            return XamlElement(
                XamlElementStartTag(name),
                content,
                XamlElementEndTag(name));
        }

        /// <summary>
        /// Creates the syntax representation of an xml text that contains a newline token with a documentation comment 
        /// exterior trivia at the end (continued documentation comment).
        /// </summary>
        /// <param name="text">The raw text within the new line.</param>
        public static XamlTextSyntax XamlNewLine(string text)
        {
            return XamlText(XamlTextNewLine(text));
        }

        /// <summary>
        /// Creates the syntax representation of an xml newline token with a documentation comment exterior trivia at 
        /// the end (continued documentation comment).
        /// </summary>
        /// <param name="text">The raw text within the new line.</param>
        public static SyntaxToken XamlTextNewLine(string text)
        {
            return XamlTextNewLine(text, true);
        }

        /// <summary>
        /// Creates a token with kind XamlTextLiteralNewLineToken.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The xml text new line value.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken XamlTextNewLine(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing)
        {
            return new SyntaxToken(
                InternalSyntax.SyntaxFactory.XamlTextNewLine(
                    leading.Node,
                    text,
                    value,
                    trailing.Node));
        }

        /// <summary>
        /// Creates the syntax representation of an xml newline token for xml documentation comments.
        /// </summary>
        /// <param name="text">The raw text within the new line.</param>
        /// <param name="continueXamlDocumentationComment">
        /// If set to true, a documentation comment exterior token will be added to the trailing trivia
        /// of the new token.</param>
        public static SyntaxToken XamlTextNewLine(string text, bool continueXamlDocumentationComment)
        {
            var token = new SyntaxToken(
                InternalSyntax.SyntaxFactory.XamlTextNewLine(
                    ElasticMarker.UnderlyingNode,
                    text,
                    text,
                    ElasticMarker.UnderlyingNode));

            if (continueXamlDocumentationComment)
                token = token.WithTrailingTrivia(token.TrailingTrivia.Add(DocumentationCommentExterior("/// ")));

            return token;
        }

        /// <summary>
        /// Generates the syntax representation of a xml text node (e.g. for xml documentation comments).
        /// </summary>
        /// <param name="value">The string literal used as the text of the xml text node.</param>
        public static XamlTextSyntax XamlText(string value)
        {
            return XamlText(XamlTextLiteral(value));
        }

        /// <summary>
        /// Generates the syntax representation of a xml text node (e.g. for xml documentation comments).
        /// </summary>
        /// <param name="textTokens">A list of text tokens used as the text of the xml text node.</param>
        public static XamlTextSyntax XamlText(params SyntaxToken[] textTokens)
        {
            return XamlText(TokenList(textTokens));
        }

        /// <summary>
        /// Generates the syntax representation of an xml text literal.
        /// </summary>
        /// <param name="value">The text used within the xml text literal.</param>
        public static SyntaxToken XamlTextLiteral(string value)
        {
            // TODO: [RobinSedlaczek] It is no compiler hot path here I think. But the contribution guide
            //       states to avoid LINQ (https://github.com/dotnet/roslyn/wiki/Contributing-Code). With
            //       XText we have a reference to System.Xaml.Linq. Isn't this rule valid here? 
            string encoded = new XText(value).ToString();

            return XamlTextLiteral(
                TriviaList(),
                encoded,
                value,
                TriviaList());
        }

        /// <summary>
        /// Generates the syntax representation of an xml text literal.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The text used within the xml text literal.</param>
        public static SyntaxToken XamlTextLiteral(string text, string value)
        {
            return new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.XamlTextLiteral(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Helper method that replaces less-than and greater-than characters with brackets. 
        /// </summary>
        /// <param name="originalToken">The original token that is to be replaced.</param>
        /// <param name="rewrittenToken">The new rewritten token.</param>
        /// <returns>Returns the new rewritten token with replaced characters.</returns>
        private static SyntaxToken XamlReplaceBracketTokens(SyntaxToken originalToken, SyntaxToken rewrittenToken)
        {
            if (rewrittenToken.IsKind(SyntaxKind.LessThanToken) && string.Equals("<", rewrittenToken.Text, StringComparison.Ordinal))
                return Token(rewrittenToken.LeadingTrivia, SyntaxKind.LessThanToken, "{", rewrittenToken.ValueText, rewrittenToken.TrailingTrivia);

            if (rewrittenToken.IsKind(SyntaxKind.GreaterThanToken) && string.Equals(">", rewrittenToken.Text, StringComparison.Ordinal))
                return Token(rewrittenToken.LeadingTrivia, SyntaxKind.GreaterThanToken, "}", rewrittenToken.ValueText, rewrittenToken.TrailingTrivia);

            return rewrittenToken;
        }

        /// <summary>
        /// Creates a trivia with kind DocumentationCommentExteriorTrivia.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        public static SyntaxTrivia DocumentationCommentExterior(string text)
        {
            return Syntax.InternalSyntax.SyntaxFactory.DocumentationCommentExteriorTrivia(text);
        }

        /// <summary>
        /// Creates an empty list of syntax nodes.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        public static SyntaxList<TNode> List<TNode>() where TNode : SyntaxNode
        {
            return default(SyntaxList<TNode>);
        }

        /// <summary>
        /// Creates a singleton list of syntax nodes.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="node">The single element node.</param>
        /// <returns></returns>
        public static SyntaxList<TNode> SingletonList<TNode>(TNode node) where TNode : SyntaxNode
        {
            return new SyntaxList<TNode>(node);
        }


        /// <summary>
        /// Creates a list of syntax nodes.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodes">A sequence of element nodes.</param>
        public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode
        {
            if (nodes != null)
            {
                var collection = nodes as ICollection<TNode>;
                var builder = (collection != null) ? new SyntaxListBuilder<TNode>(collection.Count) : SyntaxListBuilder<TNode>.Create();

                foreach (TNode node in nodes)
                {
                    builder.Add(node);
                }

                return builder.ToList();
            }

            return default(SyntaxList<TNode>);
        }

        /// <summary>
        /// Creates an empty list of tokens.
        /// </summary>
        public static SyntaxTokenList TokenList()
        {
            return default(SyntaxTokenList);
        }

        /// <summary>
        /// Creates a singleton list of tokens.
        /// </summary>
        /// <param name="token">The single token.</param>
        public static SyntaxTokenList TokenList(SyntaxToken token)
        {
            return new SyntaxTokenList(token);
        }

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        /// <param name="tokens">An array of tokens.</param>
        public static SyntaxTokenList TokenList(params SyntaxToken[] tokens)
        {
            if (tokens == null)
            {
                return default(SyntaxTokenList);
            }

            SyntaxTokenListBuilder builder = new SyntaxTokenListBuilder(tokens.Length);
            for (int i = 0; i < tokens.Length; i++)
            {
                builder.Add((InternalSyntax.SyntaxToken)tokens[i].Node);
            }

            return builder.ToList();
        }

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static SyntaxTokenList TokenList(IEnumerable<SyntaxToken> tokens)
        {
            if (tokens == null)
            {
                return default(SyntaxTokenList);
            }

            SyntaxTokenListBuilder builder = SyntaxTokenListBuilder.Create();
            foreach (SyntaxToken token in tokens)
            {
                builder.Add((InternalSyntax.SyntaxToken)token.Node);
            }

            return builder.ToList();
        }

        /// <summary>
        /// Creates a trivia from a StructuredTriviaSyntax node.
        /// </summary>
        public static SyntaxTrivia Trivia(StructuredTriviaSyntax node)
        {
            return new SyntaxTrivia(default(SyntaxToken), node.Green, position: 0, index: 0);
        }

        /// <summary>
        /// Creates an empty list of trivia.
        /// </summary>
        public static SyntaxTriviaList TriviaList()
        {
            return default(SyntaxTriviaList);
        }

        /// <summary>
        /// Creates a singleton list of trivia.
        /// </summary>
        /// <param name="trivia">A single trivia.</param>
        public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia)
        {
            return new SyntaxTriviaList(trivia);
        }

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">An array of trivia.</param>
        public static SyntaxTriviaList TriviaList(params SyntaxTrivia[] trivias)
        {
            if (trivias != null)
            {
                SyntaxTriviaListBuilder builder = new SyntaxTriviaListBuilder(trivias.Length);
                builder.Add(trivias);
                return builder.ToList();
            }

            return default(SyntaxTriviaList);
        }

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">A sequence of trivia.</param>
        public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivias)
            => SyntaxTriviaListBuilder.Create(trivias);

        /// <summary>
        /// Creates an empty separated list.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>() where TNode : SyntaxNode
        {
            return default(SeparatedSyntaxList<TNode>);
        }

        /// <summary>
        /// Creates a singleton separated list.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="node">A single node.</param>
        public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node) where TNode : SyntaxNode
        {
            return new SeparatedSyntaxList<TNode>(new SyntaxNodeOrTokenList(node, index: 0));
        }

        /// <summary>
        /// Creates a separated list of nodes from a sequence of nodes, synthesizing comma separators in between.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodes">A sequence of syntax nodes.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode
        {
            if (nodes == null)
            {
                return default(SeparatedSyntaxList<TNode>);
            }

            var collection = nodes as ICollection<TNode>;

            if (collection != null && collection.Count == 0)
            {
                return default(SeparatedSyntaxList<TNode>);
            }

            using (var enumerator = nodes.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return default(SeparatedSyntaxList<TNode>);
                }

                var firstNode = enumerator.Current;

                if (!enumerator.MoveNext())
                {
                    return SingletonSeparatedList<TNode>(firstNode);
                }

                var builder = new SeparatedSyntaxListBuilder<TNode>(collection != null ? collection.Count : 3);

                builder.Add(firstNode);

                var commaToken = Token(SyntaxKind.CommaToken);

                do
                {
                    builder.AddSeparator(commaToken);
                    builder.Add(enumerator.Current);
                }
                while (enumerator.MoveNext());

                return builder.ToList();
            }
        }

        /// <summary>
        /// Creates a separated list of nodes from a sequence of nodes and a sequence of separator tokens.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodes">A sequence of syntax nodes.</param>
        /// <param name="separators">A sequence of token to be interleaved between the nodes. The number of tokens must
        /// be one less than the number of nodes.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode> nodes, IEnumerable<SyntaxToken> separators) where TNode : SyntaxNode
        {
            // Interleave the nodes and the separators.  The number of separators must be equal to or 1 less than the number of nodes or
            // an argument exception is thrown.

            if (nodes != null)
            {
                IEnumerator<TNode> enumerator = nodes.GetEnumerator();
                SeparatedSyntaxListBuilder<TNode> builder = SeparatedSyntaxListBuilder<TNode>.Create();
                if (separators != null)
                {
                    foreach (SyntaxToken token in separators)
                    {
                        if (!enumerator.MoveNext())
                        {
                            throw new ArgumentException($"{nameof(nodes)} must not be empty.", nameof(nodes));
                        }

                        builder.Add(enumerator.Current);
                        builder.AddSeparator(token);
                    }
                }

                if (enumerator.MoveNext())
                {
                    builder.Add(enumerator.Current);
                    if (enumerator.MoveNext())
                    {
                        throw new ArgumentException($"{nameof(separators)} must have 1 fewer element than {nameof(nodes)}", nameof(separators));
                    }
                }

                return builder.ToList();
            }

            if (separators != null)
            {
                throw new ArgumentException($"When {nameof(nodes)} is null, {nameof(separators)} must also be null.", nameof(separators));
            }

            return default(SeparatedSyntaxList<TNode>);
        }

        /// <summary>
        /// Creates a separated list from a sequence of nodes and tokens, starting with a node and alternating between additional nodes and separator tokens.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodesAndTokens">A sequence of nodes or tokens, alternating between nodes and separator tokens.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<SyntaxNodeOrToken> nodesAndTokens) where TNode : SyntaxNode
        {
            return SeparatedList<TNode>(NodeOrTokenList(nodesAndTokens));
        }

        /// <summary>
        /// Creates a separated list from a <see cref="SyntaxNodeOrTokenList"/>, where the list elements start with a node and then alternate between
        /// additional nodes and separator tokens.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodesAndTokens">The list of nodes and tokens.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(SyntaxNodeOrTokenList nodesAndTokens) where TNode : SyntaxNode
        {
            if (!HasSeparatedNodeTokenPattern(nodesAndTokens))
            {
                throw new ArgumentException(CodeAnalysisResources.NodeOrTokenOutOfSequence);
            }

            if (!NodesAreCorrectType<TNode>(nodesAndTokens))
            {
                throw new ArgumentException(CodeAnalysisResources.UnexpectedTypeOfNodeInList);
            }

            return new SeparatedSyntaxList<TNode>(nodesAndTokens);
        }

        private static bool NodesAreCorrectType<TNode>(SyntaxNodeOrTokenList list)
        {
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var element = list[i];
                if (element.IsNode && !(element.AsNode() is TNode))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSeparatedNodeTokenPattern(SyntaxNodeOrTokenList list)
        {
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var element = list[i];
                if (element.IsToken == ((i & 1) == 0))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates an empty <see cref="SyntaxNodeOrTokenList"/>.
        /// </summary>
        public static SyntaxNodeOrTokenList NodeOrTokenList()
        {
            return default(SyntaxNodeOrTokenList);
        }

        /// <summary>
        /// Create a <see cref="SyntaxNodeOrTokenList"/> from a sequence of <see cref="SyntaxNodeOrToken"/>.
        /// </summary>
        /// <param name="nodesAndTokens">The sequence of nodes and tokens</param>
        public static SyntaxNodeOrTokenList NodeOrTokenList(IEnumerable<SyntaxNodeOrToken> nodesAndTokens)
        {
            if (nodesAndTokens == null)
            {
                throw new ArgumentNullException(nameof(nodesAndTokens));
            }

            var builder = new SyntaxNodeOrTokenListBuilder(8);
            builder.Add(nodesAndTokens);
            return builder.ToList();
        }

        /// <summary>
        /// Create a <see cref="SyntaxNodeOrTokenList"/> from one or more <see cref="SyntaxNodeOrToken"/>.
        /// </summary>
        /// <param name="nodesAndTokens">The nodes and tokens</param>
        public static SyntaxNodeOrTokenList NodeOrTokenList(params SyntaxNodeOrToken[] nodesAndTokens)
        {
            return NodeOrTokenList((IEnumerable<SyntaxNodeOrToken>)nodesAndTokens);
        }

        /// <summary>
        /// Creates an IdentifierNameSyntax node.
        /// </summary>
        /// <param name="name">The identifier name.</param>
        public static IdentifierNameSyntax IdentifierName(string name)
        {
            return IdentifierName(Identifier(name));
        }

        // direct access to parsing for common grammar areas

        /// <summary>
        /// Create a new syntax tree from a syntax node.
        /// </summary>
        public static SyntaxTree SyntaxTree(SyntaxNode root, ParseOptions options = null, string path = "", Encoding encoding = null)
        {
            return XamlSyntaxTree.Create((XamlSyntaxNode)root, (XamlParseOptions)options, path, encoding);
        }

        /// <summary>
        /// Produces a syntax tree by parsing the source text.
        /// </summary>
        public static SyntaxTree ParseSyntaxTree(
            string text,
            ParseOptions options = null,
            string path = "",
            Encoding encoding = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return ParseSyntaxTree(SourceText.From(text, encoding), options, path, cancellationToken);
        }

        /// <summary>
        /// Produces a syntax tree by parsing the source text.
        /// </summary>
        public static SyntaxTree ParseSyntaxTree(
            SourceText text,
            ParseOptions options = null,
            string path = "",
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return XamlSyntaxTree.ParseText(text, (XamlParseOptions)options, path, cancellationToken);
        }

        /// <summary>
        /// Parse a list of trivia rules for leading trivia.
        /// </summary>
        public static SyntaxTriviaList ParseLeadingTrivia(string text, int offset = 0)
        {
            return ParseLeadingTrivia(text, XamlParseOptions.Default, offset);
        }

        /// <summary>
        /// Parse a list of trivia rules for leading trivia.
        /// </summary>
        internal static SyntaxTriviaList ParseLeadingTrivia(string text, XamlParseOptions options, int offset = 0)
        {
            using (var lexer = new InternalSyntax.Lexer(MakeSourceText(text, offset), options))
            {
                return lexer.LexSyntaxLeadingTrivia();
            }
        }

        /// <summary>
        /// Parse a list of trivia using the parsing rules for trailing trivia.
        /// </summary>
        public static SyntaxTriviaList ParseTrailingTrivia(string text, int offset = 0)
        {
            using (var lexer = new InternalSyntax.Lexer(MakeSourceText(text, offset), XamlParseOptions.Default))
            {
                return lexer.LexSyntaxTrailingTrivia();
            }
        }

        /// <summary>
        /// Parse a XAML language token.
        /// </summary>
        /// <param name="text">The text of the token including leading and trailing trivia.</param>
        /// <param name="offset">Optional offset into text.</param>
        public static SyntaxToken ParseToken(string text, int offset = 0)
        {
            using (var lexer = new InternalSyntax.Lexer(MakeSourceText(text, offset), XamlParseOptions.Default))
            {
                return new SyntaxToken(lexer.Lex());
            }
        }

        /// <summary>
        /// Parse a sequence of XAML language tokens.
        /// </summary>
        /// <param name="text">The text of all the tokens.</param>
        /// <param name="initialTokenPosition">An integer to use as the starting position of the first token.</param>
        /// <param name="offset">Optional offset into text.</param>
        /// <param name="options">Parse options.</param>
        public static IEnumerable<SyntaxToken> ParseTokens(string text, int offset = 0, int initialTokenPosition = 0, XamlParseOptions options = null)
        {
            using (var lexer = new InternalSyntax.Lexer(MakeSourceText(text, offset), options ?? XamlParseOptions.Default))
            {
                var position = initialTokenPosition;
                while (true)
                {
                    var token = lexer.Lex();
                    yield return new SyntaxToken(parent: null, token: token, position: position, index: 0);

                    position += token.FullWidth;

                    if (token.Kind == SyntaxKind.EndOfFileToken)
                    {
                        break;
                    }
                }
            }
        }

        ///// <summary>
        ///// Parse a NameSyntax node using the grammar rule for names.
        ///// </summary>
        //public static NameSyntax ParseName(string text, int offset = 0, bool consumeFullText = true)
        //{
        //    using (var lexer = MakeLexer(text, offset))
        //    using (var parser = MakeParser(lexer))
        //    {
        //        var node = parser.ParseName();
        //        if (consumeFullText) node = parser.ConsumeUnexpectedTokens(node);
        //        return (NameSyntax)node.CreateRed();
        //    }
        //}

        ///// <summary>
        ///// Parse a TypeNameSyntax node using the grammar rule for type names.
        ///// </summary>
        //public static TypeSyntax ParseTypeName(string text, int offset = 0, bool consumeFullText = true)
        //{
        //    using (var lexer = MakeLexer(text, offset))
        //    using (var parser = MakeParser(lexer))
        //    {
        //        var node = parser.ParseTypeName();
        //        if (consumeFullText) node = parser.ConsumeUnexpectedTokens(node);
        //        return (TypeSyntax)node.CreateRed();
        //    }
        //}

        /// <summary>
        /// Helper method for wrapping a string in an SourceText.
        /// </summary>
        private static SourceText MakeSourceText(string text, int offset)
        {
            return SourceText.From(text, Encoding.UTF8).GetSubText(offset);
        }

        private static InternalSyntax.Lexer MakeLexer(string text, int offset, XamlParseOptions options = null)
        {
            return new InternalSyntax.Lexer(
                text: MakeSourceText(text, offset),
                options: options ?? XamlParseOptions.Default);
        }

        private static InternalSyntax.LanguageParser MakeParser(InternalSyntax.Lexer lexer)
        {
            return new InternalSyntax.LanguageParser(lexer, oldTree: null, changes: null);
        }

        /// <summary>
        /// Determines if two trees are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldTree">The original tree.</param>
        /// <param name="newTree">The new tree.</param>
        /// <param name="topLevel"> 
        /// If true then the trees are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent(SyntaxTree oldTree, SyntaxTree newTree, bool topLevel)
        {
            if (oldTree == null && newTree == null)
                return true;

            if (oldTree == null || newTree == null)
                return false;

            return SyntaxEquivalence.AreEquivalent(oldTree, newTree, ignoreChildNode: null, topLevel: topLevel);
        }

        /// <summary>
        /// Determines if two syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldNode">The old node.</param>
        /// <param name="newNode">The new node.</param>
        /// <param name="topLevel"> 
        /// If true then the nodes are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent(SyntaxNode oldNode, SyntaxNode newNode, bool topLevel)
        {
            return SyntaxEquivalence.AreEquivalent(oldNode, newNode, ignoreChildNode: null, topLevel: topLevel);
        }

        /// <summary>
        /// Determines if two syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldNode">The old node.</param>
        /// <param name="newNode">The new node.</param>
        /// <param name="ignoreChildNode">
        /// If specified called for every child syntax node (not token) that is visited during the comparison. 
        /// If it returns true the child is recursively visited, otherwise the child and its subtree is disregarded.
        /// </param>
        public static bool AreEquivalent(SyntaxNode oldNode, SyntaxNode newNode, Func<SyntaxKind, bool> ignoreChildNode = null)
        {
            return SyntaxEquivalence.AreEquivalent(oldNode, newNode, ignoreChildNode: ignoreChildNode, topLevel: false);
        }

        /// <summary>
        /// Determines if two syntax tokens are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldToken">The old token.</param>
        /// <param name="newToken">The new token.</param>
        public static bool AreEquivalent(SyntaxToken oldToken, SyntaxToken newToken)
        {
            return SyntaxEquivalence.AreEquivalent(oldToken, newToken);
        }

        /// <summary>
        /// Determines if two lists of tokens are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old token list.</param>
        /// <param name="newList">The new token list.</param>
        public static bool AreEquivalent(SyntaxTokenList oldList, SyntaxTokenList newList)
        {
            return SyntaxEquivalence.AreEquivalent(oldList, newList);
        }

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="topLevel"> 
        /// If true then the nodes are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, bool topLevel)
            where TNode : XamlSyntaxNode
        {
            return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);
        }

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="ignoreChildNode">
        /// If specified called for every child syntax node (not token) that is visited during the comparison. 
        /// If it returns true the child is recursively visited, otherwise the child and its subtree is disregarded.
        /// </param>
        public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, Func<SyntaxKind, bool> ignoreChildNode = null)
            where TNode : SyntaxNode
        {
            return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);
        }

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="topLevel"> 
        /// If true then the nodes are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, bool topLevel)
            where TNode : SyntaxNode
        {
            return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);
        }

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="ignoreChildNode">
        /// If specified called for every child syntax node (not token) that is visited during the comparison. 
        /// If it returns true the child is recursively visited, otherwise the child and its subtree is disregarded.
        /// </param>
        public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, Func<SyntaxKind, bool> ignoreChildNode = null)
            where TNode : SyntaxNode
        {
            return SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);
        }

        private static bool HasUnterminatedMultiLineComment(SyntaxTriviaList triviaList)
        {
            foreach (var trivia in triviaList)
            {
                if (trivia.ContainsDiagnostics && trivia.Kind() == SyntaxKind.MultiLineCommentTrivia)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

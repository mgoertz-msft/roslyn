// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Xaml
{
    public static partial class SyntaxFacts
    {
        public static bool IsKeywordKind(SyntaxKind kind)
        {
            return IsReservedKeyword(kind);
        }

        public static IEnumerable<SyntaxKind> GetReservedKeywordKinds()
        {
            for (int i = (int)SyntaxKind.NullKeyword; i <= (int)SyntaxKind.NamespaceKeyword; i++)
            {
                yield return (SyntaxKind)i;
            }
        }

        public static IEnumerable<SyntaxKind> GetKeywordKinds()
        {
            foreach (var reserved in GetReservedKeywordKinds())
            {
                yield return reserved;
            }
        }

        public static bool IsReservedKeyword(SyntaxKind kind)
        {
            return kind >= SyntaxKind.NullKeyword && kind <= SyntaxKind.NamespaceKeyword;
        }

        public static bool IsAccessibilityModifier(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.PrivateKeyword:
                case SyntaxKind.ProtectedKeyword:
                case SyntaxKind.InternalKeyword:
                case SyntaxKind.PublicKeyword:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool IsLiteral(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.IdentifierToken:
                case SyntaxKind.StringLiteralToken:
                case SyntaxKind.CharacterLiteralToken:
                case SyntaxKind.NumericLiteralToken:
                case SyntaxKind.XamlTextLiteralToken:
                case SyntaxKind.XamlTextLiteralNewLineToken:
                case SyntaxKind.XamlEntityLiteralToken:
                    //case SyntaxKind.Unknown:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAnyToken(SyntaxKind kind)
        {
            if (kind >= SyntaxKind.XamlFirst && kind < SyntaxKind.EndOfLineTrivia) return true;
            switch (kind)
            {
                case SyntaxKind.UnderscoreToken:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsTrivia(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.EndOfLineTrivia:
                case SyntaxKind.WhitespaceTrivia:
                case SyntaxKind.SingleLineCommentTrivia:
                case SyntaxKind.MultiLineCommentTrivia:
                case SyntaxKind.SingleLineDocumentationCommentTrivia:
                case SyntaxKind.MultiLineDocumentationCommentTrivia:
                case SyntaxKind.DisabledTextTrivia:
                case SyntaxKind.DocumentationCommentExteriorTrivia:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsName(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.IdentifierName:
                case SyntaxKind.GenericName:
                case SyntaxKind.QualifiedName:
                case SyntaxKind.AliasQualifiedName:
                    return true;
                default:
                    return false;
            }
        }

        public static SyntaxKind GetKeywordKind(string text)
        {
            switch (text)
            {
                case "null":
                    return SyntaxKind.NullKeyword;
                case "true":
                    return SyntaxKind.TrueKeyword;
                case "false":
                    return SyntaxKind.FalseKeyword;
                case "public":
                    return SyntaxKind.PublicKeyword;
                case "private":
                    return SyntaxKind.PrivateKeyword;
                case "internal":
                    return SyntaxKind.InternalKeyword;
                case "protected":
                    return SyntaxKind.ProtectedKeyword;
                default:
                    return SyntaxKind.None;
            }
        }

        public static string GetText(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.TildeToken:
                    return "~";
                case SyntaxKind.ExclamationToken:
                    return "!";
                case SyntaxKind.DollarToken:
                    return "$";
                case SyntaxKind.PercentToken:
                    return "%";
                case SyntaxKind.CaretToken:
                    return "^";
                case SyntaxKind.AmpersandToken:
                    return "&";
                case SyntaxKind.AsteriskToken:
                    return "*";
                case SyntaxKind.OpenParenToken:
                    return "(";
                case SyntaxKind.CloseParenToken:
                    return ")";
                case SyntaxKind.MinusToken:
                    return "-";
                case SyntaxKind.PlusToken:
                    return "+";
                case SyntaxKind.EqualsToken:
                    return "=";
                case SyntaxKind.OpenBraceToken:
                    return "{";
                case SyntaxKind.CloseBraceToken:
                    return "}";
                case SyntaxKind.OpenBracketToken:
                    return "[";
                case SyntaxKind.CloseBracketToken:
                    return "]";
                case SyntaxKind.BarToken:
                    return "|";
                case SyntaxKind.BackslashToken:
                    return "\\";
                case SyntaxKind.ColonToken:
                    return ":";
                case SyntaxKind.SemicolonToken:
                    return ";";
                case SyntaxKind.DoubleQuoteToken:
                    return "\"";
                case SyntaxKind.SingleQuoteToken:
                    return "'";
                case SyntaxKind.LessThanToken:
                    return "<";
                case SyntaxKind.CommaToken:
                    return ",";
                case SyntaxKind.GreaterThanToken:
                    return ">";
                case SyntaxKind.DotToken:
                    return ".";
                case SyntaxKind.QuestionToken:
                    return "?";
                case SyntaxKind.HashToken:
                    return "#";
                case SyntaxKind.SlashToken:
                    return "/";
                case SyntaxKind.SlashGreaterThanToken:
                    return "/>";
                case SyntaxKind.LessThanSlashToken:
                    return "</";
                case SyntaxKind.XamlCommentStartToken:
                    return "<!--";
                case SyntaxKind.XamlCommentEndToken:
                    return "-->";
                case SyntaxKind.XamlCDataStartToken:
                    return "<![CDATA[";
                case SyntaxKind.XamlCDataEndToken:
                    return "]]>";
                case SyntaxKind.XamlProcessingInstructionStartToken:
                    return "<?";
                case SyntaxKind.XamlProcessingInstructionEndToken:
                    return "?>";
                case SyntaxKind.UnderscoreToken:
                    return "_";
                default:
                    return string.Empty;
            }
        }

        public static bool IsDocumentationCommentTrivia(SyntaxKind kind)
        {
            return kind == SyntaxKind.SingleLineDocumentationCommentTrivia ||
                kind == SyntaxKind.MultiLineDocumentationCommentTrivia;
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharp.CSharpExtensions;
using VisualBasicExtensions = Microsoft.CodeAnalysis.VisualBasic.VisualBasicExtensions;
using XmlExtensions = Microsoft.CodeAnalysis.Xml.XmlExtensions;

namespace Roslyn.SyntaxVisualizer.Control
{
    public static class SyntaxKindHelper
    {
        // Helpers that return the language-specific (C# / VB) SyntaxKind of a language-agnostic
        // SyntaxNode / SyntaxToken / SyntaxTrivia.

        public static string GetKind(this SyntaxNodeOrToken nodeOrToken)
        {
            var kind = string.Empty;

            if (nodeOrToken.IsNode)
            {
                kind = nodeOrToken.AsNode().GetKind();
            }
            else
            {
                kind = nodeOrToken.AsToken().GetKind();
            }

            return kind;
        }

        public static string GetKind(this SyntaxNode node)
        {
            var kind = string.Empty;

            switch (node.Language)
            {
                case LanguageNames.CSharp:
                    kind = CSharpExtensions.Kind(node).ToString();
                    break;
                case LanguageNames.VisualBasic:
                    kind = VisualBasicExtensions.Kind(node).ToString();
                    break;
                case "Xml":
                    kind = XmlExtensions.Kind(node).ToString();
                    break;
            }

            return kind;
        }

        public static string GetKind(this SyntaxToken token)
        {
            var kind = string.Empty;

            switch (token.Language)
            {
                case LanguageNames.CSharp:
                    kind = CSharpExtensions.Kind(token).ToString();
                    break;
                case LanguageNames.VisualBasic:
                    kind = VisualBasicExtensions.Kind(token).ToString();
                    break;
                case "Xml":
                    kind = XmlExtensions.Kind(token).ToString();
                    break;
            }

            return kind;
        }

        public static string GetKind(this SyntaxTrivia trivia)
        {
            var kind = string.Empty;

            switch (trivia.Language)
            {
                case LanguageNames.CSharp:
                    kind = CSharpExtensions.Kind(trivia).ToString();
                    break;
                case LanguageNames.VisualBasic:
                    kind = VisualBasicExtensions.Kind(trivia).ToString();
                    break;
                case "Xml":
                    kind = XmlExtensions.Kind(trivia).ToString();
                    break;
            }

            return kind;
        }
    }
}

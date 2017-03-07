// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Xaml;
using Microsoft.CodeAnalysis.Xaml.Syntax;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml.Extensions
{
    internal static partial class SyntaxTreeExtensions
    {
        public static bool IsEntirelyWithinMultiLineComment(
            this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            var trivia = syntaxTree.FindTriviaAndAdjustForEndOfFile(position, cancellationToken);

            if (trivia.IsMultiLineComment())
            {
                var span = trivia.FullSpan;

                return trivia.IsCompleteMultiLineComment()
                    ? position > span.Start && position < span.End
                    : position > span.Start && position <= span.End;
            }

            return false;
        }

        private static bool AtEndOfIncompleteStringOrCharLiteral(SyntaxToken token, int position, char lastChar)
        {
            if (!token.IsKind(SyntaxKind.StringLiteralToken, SyntaxKind.CharacterLiteralToken))
            {
                throw new ArgumentException("Expected_string_or_char_literal", nameof(token));
            }

            int startLength = 1;
            return position == token.Span.End &&
                (token.Span.Length == startLength || (token.Span.Length > startLength && token.ToString().Cast<char>().LastOrDefault() != lastChar));
        }

        public static bool IsEntirelyWithinStringOrCharLiteral(
            this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            return
                syntaxTree.IsEntirelyWithinStringLiteral(position, cancellationToken) ||
                syntaxTree.IsEntirelyWithinCharLiteral(position, cancellationToken);
        }

        public static bool IsEntirelyWithinStringLiteral(
            this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            var token = syntaxTree.GetRoot(cancellationToken).FindToken(position, findInsideTrivia: true);

            // If we ask right at the end of the file, we'll get back nothing. We handle that case
            // specially for now, though SyntaxTree.FindToken should work at the end of a file.
            if (token.IsKind(SyntaxKind.EndOfFileToken))
            {
                token = token.GetPreviousToken(includeSkipped: true, includeDirectives: true);
            }

            if (token.IsKind(SyntaxKind.StringLiteralToken))
            {
                var span = token.Span;

                // cases:
                // "|"
                // "|  (e.g. incomplete string literal)
                return (position > span.Start && position < span.End)
                    || AtEndOfIncompleteStringOrCharLiteral(token, position, '"');
            }

            return false;
        }

        public static bool IsEntirelyWithinCharLiteral(
            this SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            var root = syntaxTree.GetRoot(cancellationToken) as XamlBodySyntax;
            var token = root.FindToken(position, findInsideTrivia: true);

            // If we ask right at the end of the file, we'll get back nothing.
            // We handle that case specially for now, though SyntaxTree.FindToken should
            // work at the end of a file.
            if (position == root.FullWidth())
            {
                token = root.EndOfFileToken.GetPreviousToken(includeSkipped: true, includeDirectives: true);
            }

            if (token.Kind() == SyntaxKind.CharacterLiteralToken)
            {
                var span = token.Span;

                // cases:
                // '|'
                // '|  (e.g. incomplete char literal)
                return (position > span.Start && position < span.End)
                    || AtEndOfIncompleteStringOrCharLiteral(token, position, '\'');
            }

            return false;
        }
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Shared.Utilities;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml.Extensions
{
    internal static partial class SyntaxNodeExtensions
    {
        public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind)
        {
            return node != null && CodeAnalysis.XamlExtensions.IsKind(node.Parent, kind);
        }

        public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2)
        {
            return node != null && IsKind(node.Parent, kind1, kind2);
        }

        public static bool IsParentKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
        {
            return node != null && IsKind(node.Parent, kind1, kind2, kind3);
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2)
        {
            if (node == null)
            {
                return false;
            }

            var csharpKind = node.Kind();
            return csharpKind == kind1 || csharpKind == kind2;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3)
        {
            if (node == null)
            {
                return false;
            }

            var csharpKind = node.Kind();
            return csharpKind == kind1 || csharpKind == kind2 || csharpKind == kind3;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4)
        {
            if (node == null)
            {
                return false;
            }

            var csharpKind = node.Kind();
            return csharpKind == kind1 || csharpKind == kind2 || csharpKind == kind3 || csharpKind == kind4;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4, SyntaxKind kind5)
        {
            if (node == null)
            {
                return false;
            }

            var csharpKind = node.Kind();
            return csharpKind == kind1 || csharpKind == kind2 || csharpKind == kind3 || csharpKind == kind4 || csharpKind == kind5;
        }

        public static bool IsKind(this SyntaxNode node, SyntaxKind kind1, SyntaxKind kind2, SyntaxKind kind3, SyntaxKind kind4, SyntaxKind kind5, SyntaxKind kind6)
        {
            if (node == null)
            {
                return false;
            }

            var csharpKind = node.Kind();
            return csharpKind == kind1 || csharpKind == kind2 || csharpKind == kind3 || csharpKind == kind4 || csharpKind == kind5 || csharpKind == kind6;
        }


        // Matches the following:
        //
        // (whitespace* newline)+ 
        private static readonly Matcher<SyntaxTrivia> s_oneOrMoreBlankLines;

        // Matches the following:
        // 
        // (whitespace* (single-comment|multi-comment) whitespace* newline)+ OneOrMoreBlankLines
        private static readonly Matcher<SyntaxTrivia> s_bannerMatcher;

        // Used to match the following:
        //
        // <start-of-file> (whitespace* (single-comment|multi-comment) whitespace* newline)+ blankLine*
        private static readonly Matcher<SyntaxTrivia> s_fileBannerMatcher;

        static SyntaxNodeExtensions()
        {
            var whitespace = Matcher.Repeat(Match(SyntaxKind.WhitespaceTrivia, "\\b"));
            var endOfLine = Match(SyntaxKind.EndOfLineTrivia, "\\n");
            var singleBlankLine = Matcher.Sequence(whitespace, endOfLine);

            var multiLineComment = Match(SyntaxKind.MultiLineCommentTrivia, "<!---->");
            var anyCommentMatcher = Matcher.Choice(multiLineComment);

            var commentLine = Matcher.Sequence(whitespace, anyCommentMatcher, whitespace, endOfLine);

            s_oneOrMoreBlankLines = Matcher.OneOrMore(singleBlankLine);
            s_bannerMatcher =
                Matcher.Sequence(
                    Matcher.OneOrMore(commentLine),
                    s_oneOrMoreBlankLines);
            s_fileBannerMatcher =
                Matcher.Sequence(
                    Matcher.OneOrMore(commentLine),
                    Matcher.Repeat(singleBlankLine));
        }

        private static Matcher<SyntaxTrivia> Match(SyntaxKind kind, string description)
        {
            return Matcher.Single<SyntaxTrivia>(t => t.Kind() == kind, description);
        }

        public static IEnumerable<SyntaxTrivia> GetAllPrecedingTriviaToPreviousToken(
            this SyntaxNode node, SourceText sourceText = null, 
            bool includePreviousTokenTrailingTriviaOnlyIfOnSameLine = false)
            => node.GetFirstToken().GetAllPrecedingTriviaToPreviousToken(
                sourceText, includePreviousTokenTrailingTriviaOnlyIfOnSameLine);

        /// <summary>
        /// Returns all of the trivia to the left of this token up to the previous token (concatenates
        /// the previous token's trailing trivia and this token's leading trivia).
        /// </summary>
        public static IEnumerable<SyntaxTrivia> GetAllPrecedingTriviaToPreviousToken(
            this SyntaxToken token, SourceText sourceText = null, 
            bool includePreviousTokenTrailingTriviaOnlyIfOnSameLine = false)
        {
            var prevToken = token.GetPreviousToken(includeSkipped: true);
            if (prevToken.Kind() == SyntaxKind.None)
            {
                return token.LeadingTrivia;
            }

            if (includePreviousTokenTrailingTriviaOnlyIfOnSameLine && 
                !sourceText.AreOnSameLine(prevToken, token))
            {
                return token.LeadingTrivia;
            }

            return prevToken.TrailingTrivia.Concat(token.LeadingTrivia);
        }


        public static TSyntaxNode GetNodeWithoutLeadingBlankLines<TSyntaxNode>(
            this TSyntaxNode node)
            where TSyntaxNode : SyntaxNode
        {
            return node.GetNodeWithoutLeadingBlankLines(out var blankLines);
        }

        public static TSyntaxNode GetNodeWithoutLeadingBlankLines<TSyntaxNode>(
            this TSyntaxNode node, out IEnumerable<SyntaxTrivia> strippedTrivia)
            where TSyntaxNode : SyntaxNode
        {
            var leadingTriviaToKeep = new List<SyntaxTrivia>(node.GetLeadingTrivia());

            var index = 0;
            s_oneOrMoreBlankLines.TryMatch(leadingTriviaToKeep, ref index);

            strippedTrivia = new List<SyntaxTrivia>(leadingTriviaToKeep.Take(index));

            return node.WithLeadingTrivia(leadingTriviaToKeep.Skip(index));
        }

        public static IEnumerable<SyntaxNode> GetAncestorsOrThis(this SyntaxNode node, Func<SyntaxNode, bool> predicate)
        {
            var current = node;
            while (current != null)
            {
                if (predicate(current))
                {
                    yield return current;
                }

                current = current.Parent;
            }
        }

        /// <summary>
        /// Returns child node or token that contains given position.
        /// </summary>
        /// <remarks>
        /// This is a copy of <see cref="SyntaxNode.ChildThatContainsPosition"/> that also returns the index of the child node.
        /// </remarks>
        internal static SyntaxNodeOrToken ChildThatContainsPosition(this SyntaxNode self, int position, out int childIndex)
        {
            var childList = self.ChildNodesAndTokens();

            int left = 0;
            int right = childList.Count - 1;

            while (left <= right)
            {
                int middle = left + ((right - left) / 2);
                SyntaxNodeOrToken node = childList[middle];

                var span = node.FullSpan;
                if (position < span.Start)
                {
                    right = middle - 1;
                }
                else if (position >= span.End)
                {
                    left = middle + 1;
                }
                else
                {
                    childIndex = middle;
                    return node;
                }
            }

            // we could check up front that index is within FullSpan,
            // but we wan to optimize for the common case where position is valid.
            Debug.Assert(!self.FullSpan.Contains(position), "Position is valid. How could we not find a child?");
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        public static SyntaxNode GetParent(this SyntaxNode node)
        {
            return node != null ? node.Parent : null;
        }

        public static (SyntaxToken openBrace, SyntaxToken closeBrace) GetBraces(this SyntaxNode node)
        {
            //var namespaceNode = node as NamespaceDeclarationSyntax;
            //if (namespaceNode != null)
            //{
            //    return (namespaceNode.OpenBraceToken, namespaceNode.CloseBraceToken);
            //}

            return default((SyntaxToken, SyntaxToken));
        }

        public static (SyntaxToken openBrace, SyntaxToken closeBrace) GetParentheses(this SyntaxNode node)
        {
            switch (node)
            {
                //case CastExpressionSyntax n: return (n.OpenParenToken, n.CloseParenToken);
                default: return default((SyntaxToken, SyntaxToken));
            }
        }

        public static (SyntaxToken openBrace, SyntaxToken closeBrace) GetBrackets(this SyntaxNode node)
        {
            switch (node)
            {
                //case BracketedParameterListSyntax n: return (n.OpenBracketToken, n.CloseBracketToken);
                default: return default((SyntaxToken, SyntaxToken));
            }
        }

        public static TNode ConvertToSingleLine<TNode>(this TNode node, bool useElasticTrivia = false)
            where TNode : SyntaxNode
        {
            if (node == null)
            {
                return node;
            }

            var rewriter = new SingleLineRewriter(useElasticTrivia);
            return (TNode)rewriter.Visit(node);
        }
    }
}
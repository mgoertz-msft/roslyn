// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.LanguageServices;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Xml;
using Microsoft.CodeAnalysis.Xml.Extensions;
using Microsoft.CodeAnalysis.Xml.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml
{
    internal class XamlSyntaxFactsService : AbstractSyntaxFactsService, ISyntaxFactsService
    {
        internal static readonly XamlSyntaxFactsService Instance = new XamlSyntaxFactsService();
        private const string dotToken = ".";

        private XamlSyntaxFactsService()
        {
        }

        public bool IsCaseSensitive => true;

       public StringComparer StringComparer { get; } = StringComparer.Ordinal;

        public SyntaxTrivia ElasticMarker
            => SyntaxFactory.ElasticMarker;

        public SyntaxTrivia ElasticCarriageReturnLineFeed
            => SyntaxFactory.ElasticCarriageReturnLineFeed;

        protected override IDocumentationCommentService DocumentationCommentService
            => null;

        public bool SupportsIndexingInitializer(ParseOptions options) 
            => false;

        public bool SupportsThrowExpression(ParseOptions options)
            => false;

        public SyntaxToken ParseToken(string text)
            => SyntaxFactory.ParseToken(text);

        public bool IsAwaitKeyword(SyntaxToken token)
        {
            return false;
        }

        public bool IsIdentifier(SyntaxToken token)
        {
            return token.IsKind(SyntaxKind.IdentifierToken);
        }

        public bool IsGlobalNamespaceKeyword(SyntaxToken token)
        {
            return false;
        }

        public bool IsVerbatimIdentifier(SyntaxToken token)
        {
            return false;
        }

        public bool IsOperator(SyntaxToken token)
        {
            return false;
        }

        public bool IsKeyword(SyntaxToken token)
        {
            var kind = (SyntaxKind)token.RawKind;
            return
                SyntaxFacts.IsKeywordKind(kind); // both contextual and reserved keywords
        }

        public bool IsContextualKeyword(SyntaxToken token)
        {
            return false;
        }

        public bool IsPreprocessorKeyword(SyntaxToken token)
        {
            return false;
        }

        public bool IsHashToken(SyntaxToken token)
        {
            return (SyntaxKind)token.RawKind == SyntaxKind.HashToken;
        }

        public bool IsInInactiveRegion(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            return false;
        }

        public bool IsInNonUserCode(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            return false;
        }

        public bool IsEntirelyWithinStringOrCharOrNumericLiteral(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            var xmlTree = syntaxTree as SyntaxTree;
            if (xmlTree == null)
            {
                return false;
            }

            return xmlTree.IsEntirelyWithinStringOrCharLiteral(position, cancellationToken);
        }

        public bool IsDirective(SyntaxNode node)
        {
            return false;
        }

        public bool TryGetExternalSourceInfo(SyntaxNode node, out ExternalSourceInfo info)
        {
            info = default;
            return false;
        }

        public bool IsRightSideOfQualifiedName(SyntaxNode node)
        {
            var name = node as SimpleNameSyntax;
            return name.IsRightSideOfQualifiedName();
        }

        public bool IsNameOfMemberAccessExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsObjectCreationExpressionType(SyntaxNode node)
        {
            return false;
        }

        public bool IsAttributeName(SyntaxNode node)
        {
            return false;
        }

        public bool IsInvocationExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsAnonymousFunction(SyntaxNode node)
        {
            return false;
        }

        public bool IsGenericName(SyntaxNode node)
        {
            return false;
        }

        public bool IsNamedParameter(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetDefaultOfParameter(SyntaxNode node)
            => default;

        public SyntaxNode GetParameterList(SyntaxNode node)
            => default;

        public bool IsSkippedTokensTrivia(SyntaxNode node)
        {
            return node is SkippedTokensTriviaSyntax;
        }

        public bool HasIncompleteParentMember(SyntaxNode node)
        {
            return false;
        }

        public SyntaxToken GetIdentifierOfGenericName(SyntaxNode genericName)
        {
            return default;
        }

        public bool IsUsingDirectiveName(SyntaxNode node)
        {
            return false;
        }

        public bool IsForEachStatement(SyntaxNode node)
        {
            return false;
        }

        public bool IsLockStatement(SyntaxNode node)
        {
            return false;
        }

        public bool IsUsingStatement(SyntaxNode node)
            => false;

        public bool IsReturnStatement(SyntaxNode node)
            => false;

        public bool IsStatement(SyntaxNode node)
            => false;
 
        public bool IsParameter(SyntaxNode node)
            => false;
 
        public bool IsVariableDeclarator(SyntaxNode node)
            => false;

        public bool IsMethodBody(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetExpressionOfReturnStatement(SyntaxNode node)
            => default;

        public bool IsThisConstructorInitializer(SyntaxToken token)
        {
            return false;
        }

        public bool IsBaseConstructorInitializer(SyntaxToken token)
        {
            return false;
        }

        public bool IsQueryExpression(SyntaxNode node)
            => false;

        public bool IsThrowExpression(SyntaxNode node)
            => false;

        public bool IsPredefinedType(SyntaxToken token)
            => TryGetPredefinedType(token, out var actualType) && actualType != PredefinedType.None;

        public bool IsPredefinedType(SyntaxToken token, PredefinedType type)
            => TryGetPredefinedType(token, out var actualType) && actualType == type;

        public bool TryGetPredefinedType(SyntaxToken token, out PredefinedType type)
        {
            type = GetPredefinedType(token);
            return type != PredefinedType.None;
        }

        private PredefinedType GetPredefinedType(SyntaxToken token)
        {
            switch ((SyntaxKind)token.RawKind)
            {
                //case SyntaxKind.BoolKeyword:
                //    return PredefinedType.Boolean;
                //case SyntaxKind.ByteKeyword:
                //    return PredefinedType.Byte;
                //case SyntaxKind.SByteKeyword:
                //    return PredefinedType.SByte;
                //case SyntaxKind.IntKeyword:
                //    return PredefinedType.Int32;
                //case SyntaxKind.UIntKeyword:
                //    return PredefinedType.UInt32;
                //case SyntaxKind.ShortKeyword:
                //    return PredefinedType.Int16;
                //case SyntaxKind.UShortKeyword:
                //    return PredefinedType.UInt16;
                //case SyntaxKind.LongKeyword:
                //    return PredefinedType.Int64;
                //case SyntaxKind.ULongKeyword:
                //    return PredefinedType.UInt64;
                //case SyntaxKind.FloatKeyword:
                //    return PredefinedType.Single;
                //case SyntaxKind.DoubleKeyword:
                //    return PredefinedType.Double;
                //case SyntaxKind.DecimalKeyword:
                //    return PredefinedType.Decimal;
                //case SyntaxKind.StringKeyword:
                //    return PredefinedType.String;
                //case SyntaxKind.CharKeyword:
                //    return PredefinedType.Char;
                //case SyntaxKind.ObjectKeyword:
                //    return PredefinedType.Object;
                //case SyntaxKind.VoidKeyword:
                //    return PredefinedType.Void;
                default:
                    return PredefinedType.None;
            }
        }

        public bool IsPredefinedOperator(SyntaxToken token)
        {
            return TryGetPredefinedOperator(token, out var actualOperator) && actualOperator != PredefinedOperator.None;
        }

        public bool IsPredefinedOperator(SyntaxToken token, PredefinedOperator op)
        {
            return TryGetPredefinedOperator(token, out var actualOperator) && actualOperator == op;
        }

        public bool TryGetPredefinedOperator(SyntaxToken token, out PredefinedOperator op)
        {
            op = GetPredefinedOperator(token);
            return op != PredefinedOperator.None;
        }

        private PredefinedOperator GetPredefinedOperator(SyntaxToken token)
        {
            return PredefinedOperator.None;
        }

        public string GetText(int kind)
        {
            return SyntaxFacts.GetText((SyntaxKind)kind);
        }

        public bool IsIdentifierStartCharacter(char c)
        {
            return SyntaxFacts.IsIdentifierStartCharacter(c);
        }

        public bool IsIdentifierPartCharacter(char c)
        {
            return SyntaxFacts.IsIdentifierPartCharacter(c);
        }

        public bool IsIdentifierEscapeCharacter(char c)
        {
            return c == '@';
        }

        public bool IsValidIdentifier(string identifier)
        {
            var token = SyntaxFactory.ParseToken(identifier);
            return IsIdentifier(token) && !token.ContainsDiagnostics && token.ToString().Length == identifier.Length;
        }

        public bool IsVerbatimIdentifier(string identifier)
        {
            return false;
        }

        public bool IsTypeCharacter(char c)
        {
            return false;
        }

        public bool IsStartOfUnicodeEscapeSequence(char c)
        {
            return c == '\\';
        }

        public bool IsLiteral(SyntaxToken token)
        {
            switch (token.Kind())
            {
                case SyntaxKind.NumericLiteralToken:
                case SyntaxKind.CharacterLiteralToken:
                case SyntaxKind.StringLiteralToken:
                case SyntaxKind.NullKeyword:
                case SyntaxKind.TrueKeyword:
                case SyntaxKind.FalseKeyword:
                    return true;
            }

            return false;
        }

        public bool IsStringLiteralOrInterpolatedStringLiteral(SyntaxToken token)
        {
            return token.IsKind(SyntaxKind.StringLiteralToken);
        }

        public bool IsNumericLiteralExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsTypeNamedVarInVariableOrFieldDeclaration(SyntaxToken token, SyntaxNode parent)
        {
            return false;
        }

        public bool IsTypeNamedDynamic(SyntaxToken token, SyntaxNode parent)
        {
            return false;
        }

        public bool IsBindableToken(SyntaxToken token)
        {
            return false;
        }

        public bool IsSimpleMemberAccessExpression(SyntaxNode node)
            => false;

        public bool IsConditionalMemberAccessExpression(SyntaxNode node)
            => false;

        public bool IsPointerMemberAccessExpression(SyntaxNode node)
            => false;

        public void GetNameAndArityOfSimpleName(SyntaxNode node, out string name, out int arity)
        {
            name = null;
            arity = 0;

            if (node is SimpleNameSyntax simpleName)
            {
                name = simpleName.Identifier.ValueText;
                //arity = simpleName.Arity;
            }
        }

        public bool LooksGeneric(SyntaxNode simpleName)
            => false;

        public SyntaxNode GetTargetOfMemberBinding(SyntaxNode node)
            => default;

        public SyntaxNode GetExpressionOfMemberAccessExpression(SyntaxNode node, bool allowImplicitTarget)
            => default;

        public SyntaxNode GetExpressionOfConditionalAccessExpression(SyntaxNode node)
            => default;

        public SyntaxNode GetExpressionOfElementAccessExpression(SyntaxNode node)
            => default;

        public SyntaxNode GetArgumentListOfElementAccessExpression(SyntaxNode node)
            => default;

        public SyntaxNode GetExpressionOfInterpolation(SyntaxNode node)
            => default;

        public bool IsInStaticContext(SyntaxNode node)
        {
            return false;
        }

        public bool IsInNamespaceOrTypeContext(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetExpressionOfArgument(SyntaxNode node)
        {
            return default;
        }

        public RefKind GetRefKindOfArgument(SyntaxNode node)
            => RefKind.None;

        public bool IsSimpleArgument(SyntaxNode node)
        {
            return false;
        }


        public bool IsInConstantContext(SyntaxNode node)
        {
            return false;
        }

        public bool IsInConstructor(SyntaxNode node)
        {
            return false;
        }

        public bool IsUnsafeContext(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetNameOfAttribute(SyntaxNode node)
        {
            return default;
        }

        public bool IsAttribute(SyntaxNode node)
        {
            return false;
        }

        public bool IsAttributeNamedArgumentIdentifier(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetContainingTypeDeclaration(SyntaxNode root, int position)
        {
            return default;
        }

        public SyntaxNode GetContainingVariableDeclaratorOfFieldDeclaration(SyntaxNode node)
        {
            throw ExceptionUtilities.Unreachable;
        }

        public SyntaxToken FindTokenOnLeftOfPosition(
            SyntaxNode node, int position, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
        {
            return node.FindTokenOnLeftOfPosition(position, includeSkipped, includeDirectives, includeDocumentationComments);
        }

        public SyntaxToken FindTokenOnRightOfPosition(
            SyntaxNode node, int position, bool includeSkipped, bool includeDirectives, bool includeDocumentationComments)
        {
            return node.FindTokenOnRightOfPosition(position, includeSkipped, includeDirectives, includeDocumentationComments);
        }

        public bool IsObjectCreationExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsObjectInitializerNamedAssignmentIdentifier(SyntaxNode node)
        {
            return IsObjectInitializerNamedAssignmentIdentifier(node, out var unused);
        }

        public bool IsObjectInitializerNamedAssignmentIdentifier(
            SyntaxNode node, out SyntaxNode initializedInstance)
        {
            initializedInstance = default;
            return false;
        }

        public bool IsElementAccessExpression(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode ConvertToSingleLine(SyntaxNode node, bool useElasticTrivia = false)
            => node.ConvertToSingleLine(useElasticTrivia);

        public SyntaxToken ToIdentifierToken(string name)
        {
            return SyntaxFactory.Identifier(name);
        }

        public SyntaxNode Parenthesize(SyntaxNode expression, bool includeElasticTrivia)
        {
            // TODO: return ((ExpressionSyntax)expression).Parenthesize(includeElasticTrivia);
            return expression;
        }

        public bool IsIndexerMemberCRef(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetContainingMemberDeclaration(SyntaxNode root, int position, bool useFullSpan = true)
        {
            return default;
        }

        public bool IsMethodLevelMember(SyntaxNode node)
        {
            return false;
        }

        public bool IsTopLevelNodeWithMembers(SyntaxNode node)
        {
            return false;
        }

        public string GetDisplayName(SyntaxNode node, DisplayNameOptions options, string rootNamespace = null)
        {
            if (node == null)
            {
                return string.Empty;
            }

            var pooled = PooledStringBuilder.GetInstance();
            var builder = pooled.Builder;

            return pooled.ToStringAndFree();
        }

        private static string GetName(SyntaxNode node, DisplayNameOptions options)
        {
            const string missingTokenPlaceholder = "?";

            switch (node.Kind())
            {
                case SyntaxKind.XmlBody:
                    return null;
                case SyntaxKind.IdentifierName:
                    var identifier = ((IdentifierNameSyntax)node).Identifier;
                    return identifier.IsMissing ? missingTokenPlaceholder : identifier.Text;
                case SyntaxKind.QualifiedName:
                    var qualified = (QualifiedNameSyntax)node;
                    return GetName(qualified.Left, options) + dotToken + GetName(qualified.Right, options);
            }

            return null;
        }

        public List<SyntaxNode> GetMethodLevelMembers(SyntaxNode root)
        {
            var list = new List<SyntaxNode>();

            return list;
        }
        public TextSpan GetMemberBodySpanForSpeculativeBinding(SyntaxNode node)
        {
            return default;
        }

        public bool ContainsInMemberBody(SyntaxNode node, TextSpan span)
        {
            return false;
        }

        public int GetMethodLevelMemberId(SyntaxNode root, SyntaxNode node)
        {
            Contract.Requires(root.SyntaxTree == node.SyntaxTree);

            int currentId = 0;
            return currentId;
        }

        public SyntaxNode GetMethodLevelMember(SyntaxNode root, int memberId)
        {
            return default;
        }

        public SyntaxNode GetBindableParent(SyntaxToken token)
        {
            return default;
        }

        public IEnumerable<SyntaxNode> GetConstructors(SyntaxNode root, CancellationToken cancellationToken)
        {
            return SpecializedCollections.EmptyEnumerable<SyntaxNode>();
        }

        public bool TryGetCorrespondingOpenBrace(SyntaxToken token, out SyntaxToken openBrace)
        {
            if (token.Kind() == SyntaxKind.CloseBraceToken)
            {
                var tuple = token.Parent.GetBraces();

                openBrace = tuple.openBrace;
                return openBrace.Kind() == SyntaxKind.OpenBraceToken;
            }

            openBrace = default;
            return false;
        }

        public TextSpan GetInactiveRegionSpanAroundPosition(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            var trivia = syntaxTree.GetRoot(cancellationToken).FindTrivia(position, findInsideTrivia: false);
            if (trivia.Kind() == SyntaxKind.DisabledTextTrivia)
            {
                return trivia.FullSpan;
            }

            var token = syntaxTree.FindTokenOrEndToken(position, cancellationToken);
            if (token.Kind() == SyntaxKind.EndOfFileToken)
            {
                var triviaList = token.LeadingTrivia;
                foreach (var triviaTok in triviaList.Reverse())
                {
                    if (triviaTok.Span.Contains(position))
                    {
                        return default;
                    }

                    if (triviaTok.Span.End < position)
                    {
                        if (!triviaTok.HasStructure)
                        {
                            return default;
                        }
                    }
                }
            }

            return default;
        }

        public string GetNameForArgument(SyntaxNode argument)
        {
            return string.Empty;
        }

        public bool IsLeftSideOfDot(SyntaxNode node)
        {
            return !(node as QualifiedNameSyntax).Left.IsMissing;
        }

        public SyntaxNode GetRightSideOfDot(SyntaxNode node)
        {
            return (node as QualifiedNameSyntax).Right;
        }

        public bool IsLeftSideOfAssignment(SyntaxNode node)
        {
            return false;
        }

        public bool IsLeftSideOfAnyAssignment(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetRightHandSideOfAssignment(SyntaxNode node)
        {
            return default;
        }

        public bool IsInferredAnonymousObjectMemberDeclarator(SyntaxNode node)
        {
            return false;
        }

        public bool IsOperandOfIncrementExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsOperandOfDecrementExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsOperandOfIncrementOrDecrementExpression(SyntaxNode node)
        {
            return IsOperandOfIncrementExpression(node) || IsOperandOfDecrementExpression(node);
        }

        public SyntaxList<SyntaxNode> GetContentsOfInterpolatedString(SyntaxNode interpolatedString)
        {
            return default;
        }

        public override bool IsStringLiteral(SyntaxToken token)
            => token.IsKind(SyntaxKind.StringLiteralToken);

        public override bool IsInterpolatedStringTextToken(SyntaxToken token)
            => false;

        public bool IsStringLiteralExpression(SyntaxNode node)
            => false;

        public bool IsVerbatimStringLiteral(SyntaxToken token)
            => false;

        public bool IsNumericLiteral(SyntaxToken token)
            => token.Kind() == SyntaxKind.NumericLiteralToken;

        public bool IsCharacterLiteral(SyntaxToken token)
            => token.Kind() == SyntaxKind.CharacterLiteralToken;

        public SeparatedSyntaxList<SyntaxNode> GetArgumentsOfInvocationExpression(SyntaxNode invocationExpression)
            => default;

        public SeparatedSyntaxList<SyntaxNode> GetArgumentsOfObjectCreationExpression(SyntaxNode invocationExpression)
            => default;

        public SeparatedSyntaxList<SyntaxNode> GetArgumentsOfArgumentList(SyntaxNode argumentList)
            => default;

        public bool IsRegularComment(SyntaxTrivia trivia)
            => trivia.IsRegularComment();

        public bool IsDocumentationComment(SyntaxTrivia trivia)
            => false;

        public bool IsElastic(SyntaxTrivia trivia)
            => trivia.IsElastic();

        public bool IsDocumentationCommentExteriorTrivia(SyntaxTrivia trivia)
            => false;

        public bool IsDocumentationComment(SyntaxNode node)
            => false;

        public bool IsUsingOrExternOrImport(SyntaxNode node)
        {
            return false;
        }

        public bool IsGlobalAttribute(SyntaxNode node)
        {
            return false;
        }

        private static bool IsMemberDeclaration(SyntaxNode node)
        {
            return false;
        }

        public bool IsDeclaration(SyntaxNode node)
        {
            return false;
        }

        private static readonly SyntaxAnnotation s_annotation = new SyntaxAnnotation();

        public void AddFirstMissingCloseBrace(
            SyntaxNode root, SyntaxNode contextNode, 
            out SyntaxNode newRoot, out SyntaxNode newContextNode)
        {
            // First, annotate the context node in the tree so that we can find it again
            // after we've done all the rewriting.
            // var currentRoot = root.ReplaceNode(contextNode, contextNode.WithAdditionalAnnotations(s_annotation));
            newRoot = new AddFirstMissingCloseBaceRewriter(contextNode).Visit(root);
            newContextNode = newRoot.GetAnnotatedNodes(s_annotation).Single();
        }

        public SyntaxNode GetObjectCreationInitializer(SyntaxNode objectCreationExpression)
            => default;

        public SyntaxNode GetObjectCreationType(SyntaxNode node)
            => default;

        public bool IsSimpleAssignmentStatement(SyntaxNode statement)
        {
            return false;
        }

        public void GetPartsOfAssignmentStatement(
            SyntaxNode statement, out SyntaxNode left, out SyntaxToken operatorToken, out SyntaxNode right)
        {
            left = default;
            operatorToken = default;
            right = default;
        }

        public SyntaxNode GetNameOfMemberAccessExpression(SyntaxNode memberAccessExpression)
            => default;

        public SyntaxToken GetOperatorTokenOfMemberAccessExpression(SyntaxNode memberAccessExpression)
            => default;

        public void GetPartsOfMemberAccessExpression(SyntaxNode node, out SyntaxNode expression, out SyntaxNode name)
        {
            expression = default;
            name = default;
        }

        public SyntaxToken GetIdentifierOfSimpleName(SyntaxNode node)
        {
            return ((SimpleNameSyntax)node).Identifier;
        }

        public SyntaxToken GetIdentifierOfVariableDeclarator(SyntaxNode node)
        {
            return default;
        }

        public bool IsIdentifierName(SyntaxNode node)
            => node.IsKind(SyntaxKind.IdentifierName);

        public bool IsLocalDeclarationStatement(SyntaxNode node)
        {
            return false;
        }

        public bool IsDeclaratorOfLocalDeclarationStatement(SyntaxNode declarator, SyntaxNode localDeclarationStatement)
        {
            return false;
        }

        public bool AreEquivalent(SyntaxToken token1, SyntaxToken token2)
        {
            return SyntaxFactory.AreEquivalent(token1, token2);
        }

        public bool AreEquivalent(SyntaxNode node1, SyntaxNode node2)
        {
            return SyntaxFactory.AreEquivalent(node1, node2);
        }

        public bool IsExpressionOfInvocationExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsExpressionOfAwaitExpression(SyntaxNode node)
        {
            return false;
        }

        public bool IsExpressionOfMemberAccessExpression(SyntaxNode node)
        {
            return false;
        }

        public SyntaxNode GetExpressionOfInvocationExpression(SyntaxNode node)
            => default;

        public SyntaxNode GetExpressionOfAwaitExpression(SyntaxNode node)
            => default;

        public bool IsPossibleTupleContext(SyntaxTree syntaxTree, int position, CancellationToken cancellationToken)
        {
            return false;
        }

        public SyntaxNode GetExpressionOfExpressionStatement(SyntaxNode node)
            => default;

        public bool IsNullLiteralExpression(SyntaxNode node)
            => false;

        public bool IsDefaultLiteralExpression(SyntaxNode node)
            => false;

        public bool IsBinaryExpression(SyntaxNode node)
            => false;

        public void GetPartsOfBinaryExpression(SyntaxNode node, out SyntaxNode left, out SyntaxNode right)
        {
            left = default;
            right = default;
        }

        public void GetPartsOfConditionalExpression(SyntaxNode node, out SyntaxNode condition, out SyntaxNode whenTrue, out SyntaxNode whenFalse)
        {
            condition = default;
            whenTrue = default;
            whenFalse = default;
        }

        public SyntaxNode WalkDownParentheses(SyntaxNode node)
            => node;

        public bool IsLogicalAndExpression(SyntaxNode node)
            => false;

        public bool IsLogicalNotExpression(SyntaxNode node)
            => false;

        public SyntaxNode GetOperandOfPrefixUnaryExpression(SyntaxNode node)
            => default;

        public SyntaxNode GetNextExecutableStatement(SyntaxNode statement)
            => statement;

        public override bool IsWhitespaceTrivia(SyntaxTrivia trivia)
            => trivia.IsWhitespace();

        public override bool IsEndOfLineTrivia(SyntaxTrivia trivia)
            => trivia.IsEndOfLine();

        public override bool IsSingleLineCommentTrivia(SyntaxTrivia trivia)
            => trivia.IsSingleLineComment();

        public override bool IsMultiLineCommentTrivia(SyntaxTrivia trivia)
            => trivia.IsMultiLineComment();

        public override bool IsShebangDirectiveTrivia(SyntaxTrivia trivia)
            => false;

        public override bool IsPreprocessorDirective(SyntaxTrivia trivia)
            => false;

        private class AddFirstMissingCloseBaceRewriter: XmlSyntaxRewriter
        {
            private readonly SyntaxNode _contextNode; 
            private bool _seenContextNode = false;
            private bool _addedFirstCloseCurly = false;

            public AddFirstMissingCloseBaceRewriter(SyntaxNode contextNode)
            {
                _contextNode = contextNode;
            }

            public override SyntaxNode Visit(SyntaxNode node)
            {
                if (node == _contextNode)
                {
                    _seenContextNode = true;

                    // Annotate the context node so we can find it again in the new tree
                    // after we've added the close curly.
                    return node.WithAdditionalAnnotations(s_annotation);
                }

                // rewrite this node normally.
                var rewritten = base.Visit(node);
                if (rewritten == node)
                {
                    return rewritten;
                }

                // This node changed.  That means that something underneath us got
                // rewritten.  (i.e. we added the annotation to the context node).
                Debug.Assert(_seenContextNode);

                // Ok, we're past the context node now.  See if this is a node with 
                // curlies.  If so, if it has a missing close curly then add in the 
                // missing curly.  Also, even if it doesn't have missing curlies, 
                // then still ask to format its close curly to make sure all the 
                // curlies up the stack are properly formatted.
                var braces = rewritten.GetBraces();
                if (braces.openBrace.Kind() == SyntaxKind.None && 
                    braces.closeBrace.Kind() == SyntaxKind.None)
                {
                    // Not an item with braces.  Just pass it up.
                    return rewritten;
                }

                // See if the close brace is missing.  If it's the first missing one 
                // we're seeing then definitely add it.
                if (braces.closeBrace.IsMissing)
                {
                    if (!_addedFirstCloseCurly)
                    {
                        var closeBrace = SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
                            .WithAdditionalAnnotations(Formatter.Annotation);
                        rewritten = rewritten.ReplaceToken(braces.closeBrace, closeBrace);
                        _addedFirstCloseCurly = true;
                    }
                }
                else
                {
                    // Ask for the close brace to be formatted so that all the braces
                    // up the spine are in the right location.
                    rewritten = rewritten.ReplaceToken(braces.closeBrace,
                        braces.closeBrace.WithAdditionalAnnotations(Formatter.Annotation));
                }

                return rewritten;
            }
        }
        public bool IsOnTypeHeader(SyntaxNode root, int position)
        {
            return false;
        }

        public bool IsBetweenTypeMembers(SourceText sourceText, SyntaxNode root, int position)
        {
            return false;
        }

        public ImmutableArray<SyntaxNode> GetSelectedMembers(SyntaxNode root, TextSpan textSpan)
            => ImmutableArray<SyntaxNode>.Empty;
 
        protected override bool ContainsInterleavedDirective(TextSpan span, SyntaxToken token, CancellationToken cancellationToken)
            => false;

        public SyntaxTokenList GetModifiers(SyntaxNode node)
            => default;

        public SyntaxNode WithModifiers(SyntaxNode node, SyntaxTokenList modifiers)
            => default;

        public bool IsLiteralExpression(SyntaxNode node)
            => false;

        public SeparatedSyntaxList<SyntaxNode> GetVariablesOfLocalDeclarationStatement(SyntaxNode node)
            => default;

        public SyntaxNode GetInitializerOfVariableDeclarator(SyntaxNode node)
            => default;

        public SyntaxNode GetValueOfEqualsValueClause(SyntaxNode node)
            => default;

        public bool IsExecutableBlock(SyntaxNode node)
            => false;

        public SyntaxList<SyntaxNode> GetExecutableBlockStatements(SyntaxNode node)
            => default;

        public SyntaxNode FindInnermostCommonExecutableBlock(IEnumerable<SyntaxNode> nodes)
            => default;
    }
}

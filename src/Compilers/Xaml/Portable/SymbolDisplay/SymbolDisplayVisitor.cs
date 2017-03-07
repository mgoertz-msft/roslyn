// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.SymbolDisplay;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml
{
    internal partial class SymbolDisplayVisitor : AbstractSymbolDisplayVisitor
    {
        private readonly bool _escapeKeywordIdentifiers;
        private IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> _lazyAliasMap;

        internal SymbolDisplayVisitor(
            ArrayBuilder<SymbolDisplayPart> builder,
            SymbolDisplayFormat format,
            SemanticModel semanticModelOpt,
            int positionOpt)
            : base(builder, format, true, semanticModelOpt, positionOpt)
        {
            _escapeKeywordIdentifiers = format.MiscellaneousOptions.IncludesOption(SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers);
        }

        private SymbolDisplayVisitor(
            ArrayBuilder<SymbolDisplayPart> builder,
            SymbolDisplayFormat format,
            SemanticModel semanticModelOpt,
            int positionOpt,
            bool escapeKeywordIdentifiers,
            IDictionary<INamespaceOrTypeSymbol, IAliasSymbol> aliasMap,
            bool isFirstSymbolVisited)
            : base(builder, format, isFirstSymbolVisited, semanticModelOpt, positionOpt)
        {
            _escapeKeywordIdentifiers = escapeKeywordIdentifiers;
            _lazyAliasMap = aliasMap;
        }

        protected override AbstractSymbolDisplayVisitor MakeNotFirstVisitor()
        {
            return new SymbolDisplayVisitor(
                this.builder,
                this.format,
                this.semanticModelOpt,
                this.positionOpt,
                _escapeKeywordIdentifiers,
                _lazyAliasMap,
                isFirstSymbolVisited: false);
        }

        internal SymbolDisplayPart CreatePart(SymbolDisplayPartKind kind, ISymbol symbol, string text)
        {
            text = (text == null) ? "?" :
                   (_escapeKeywordIdentifiers && IsEscapable(kind)) ? EscapeIdentifier(text) : text;

            return new SymbolDisplayPart(kind, symbol, text);
        }

        private static bool IsEscapable(SymbolDisplayPartKind kind)
        {
            switch (kind)
            {
                case SymbolDisplayPartKind.AliasName:
                case SymbolDisplayPartKind.ClassName:
                case SymbolDisplayPartKind.StructName:
                case SymbolDisplayPartKind.InterfaceName:
                case SymbolDisplayPartKind.EnumName:
                case SymbolDisplayPartKind.DelegateName:
                case SymbolDisplayPartKind.TypeParameterName:
                case SymbolDisplayPartKind.MethodName:
                case SymbolDisplayPartKind.PropertyName:
                case SymbolDisplayPartKind.FieldName:
                case SymbolDisplayPartKind.LocalName:
                case SymbolDisplayPartKind.NamespaceName:
                case SymbolDisplayPartKind.ParameterName:
                    return true;
                default:
                    return false;
            }
        }

        private static string EscapeIdentifier(string identifier)
        {
            var kind = SyntaxFacts.GetKeywordKind(identifier);
            return kind == SyntaxKind.None
                ? identifier
                : $"@{identifier}";
        }

        public override void VisitAssembly(IAssemblySymbol symbol)
        {
            var text = format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameOnly
                ? symbol.Identity.Name
                : symbol.Identity.GetDisplayName();

            builder.Add(CreatePart(SymbolDisplayPartKind.AssemblyName, symbol, text));
        }

        public override void VisitModule(IModuleSymbol symbol)
        {
            builder.Add(CreatePart(SymbolDisplayPartKind.ModuleName, symbol, symbol.Name));
        }

        public override void VisitNamespace(INamespaceSymbol symbol)
        {
            //if (this.IsMinimizing)
            //{
            //    if (TryAddAlias(symbol, builder))
            //    {
            //        return;
            //    }

            //    MinimallyQualify(symbol);
            //    return;
            //}

            if (isFirstSymbolVisited && format.KindOptions.IncludesOption(SymbolDisplayKindOptions.IncludeNamespaceKeyword))
            {
                AddKeyword(SyntaxKind.NamespaceKeyword);
                AddSpace();
            }

            if (format.TypeQualificationStyle == SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
            {
                var containingNamespace = symbol.ContainingNamespace;
                if (ShouldVisitNamespace(containingNamespace))
                {
                    containingNamespace.Accept(this.NotFirstVisitor);
                    AddPunctuation(SyntaxKind.DotToken);
                }
            }

            builder.Add(CreatePart(SymbolDisplayPartKind.NamespaceName, symbol, symbol.Name));
        }

        public override void VisitLocal(ILocalSymbol symbol)
        {
            if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
            {
                symbol.Type.Accept(this);
                AddSpace();
            }

            builder.Add(CreatePart(SymbolDisplayPartKind.LocalName, symbol, symbol.Name));
        }

        public override void VisitDiscard(IDiscardSymbol symbol)
        {
            if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
            {
                symbol.Type.Accept(this);
                AddSpace();
            }

            builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, symbol, "_"));
        }

        public override void VisitLabel(ILabelSymbol symbol)
        {
            builder.Add(CreatePart(SymbolDisplayPartKind.LabelName, symbol, symbol.Name));
        }

        public override void VisitAlias(IAliasSymbol symbol)
        {
            builder.Add(CreatePart(SymbolDisplayPartKind.AliasName, symbol, symbol.Name));

            if (format.LocalOptions.IncludesOption(SymbolDisplayLocalOptions.IncludeType))
            {
                // ???
                AddPunctuation(SyntaxKind.EqualsToken);
                symbol.Target.Accept(this);
            }
        }

        protected override void AddSpace()
        {
            builder.Add(CreatePart(SymbolDisplayPartKind.Space, null, " "));
        }

        private void AddPunctuation(SyntaxKind punctuationKind)
        {
            builder.Add(CreatePart(SymbolDisplayPartKind.Punctuation, null, SyntaxFacts.GetText(punctuationKind)));
        }

        private void AddKeyword(SyntaxKind keywordKind)
        {
            builder.Add(CreatePart(SymbolDisplayPartKind.Keyword, null, SyntaxFacts.GetText(keywordKind)));
        }

        private void AddAccessibilityIfRequired(ISymbol symbol)
        {
            INamedTypeSymbol containingType = symbol.ContainingType;

            // this method is only called for members and they should have a containingType or a containing symbol should be a TypeSymbol.
            Debug.Assert((object)containingType != null || (symbol.ContainingSymbol is ITypeSymbol));

            if (format.MemberOptions.IncludesOption(SymbolDisplayMemberOptions.IncludeAccessibility) &&
                (containingType == null ||
                 (containingType.TypeKind != TypeKind.Interface && !IsEnumMember(symbol))))
            {
                AddAccessibility(symbol);
            }
        }

        private void AddAccessibility(ISymbol symbol)
        {
            switch (symbol.DeclaredAccessibility)
            {
                case Accessibility.Private:
                    AddKeyword(SyntaxKind.PrivateKeyword);
                    break;
                case Accessibility.Internal:
                    AddKeyword(SyntaxKind.InternalKeyword);
                    break;
                case Accessibility.ProtectedAndInternal:
                case Accessibility.Protected:
                    AddKeyword(SyntaxKind.ProtectedKeyword);
                    break;
                case Accessibility.ProtectedOrInternal:
                    AddKeyword(SyntaxKind.ProtectedKeyword);
                    AddSpace();
                    AddKeyword(SyntaxKind.InternalKeyword);
                    break;
                case Accessibility.Public:
                    AddKeyword(SyntaxKind.PublicKeyword);
                    break;
                default:
                    throw ExceptionUtilities.UnexpectedValue(symbol.DeclaredAccessibility);
            }

            AddSpace();
        }

        private bool ShouldVisitNamespace(ISymbol containingSymbol)
        {
            var namespaceSymbol = containingSymbol as INamespaceSymbol;
            if (namespaceSymbol == null)
            {
                return false;
            }

            if (format.TypeQualificationStyle != SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces)
            {
                return false;
            }

            return
                !namespaceSymbol.IsGlobalNamespace ||
                format.GlobalNamespaceStyle == SymbolDisplayGlobalNamespaceStyle.Included;
        }

        private bool IncludeNamedType(INamedTypeSymbol namedType)
        {
            return
                namedType != null &&
                (!namedType.IsScriptClass || format.CompilerInternalOptions.IncludesOption(SymbolDisplayCompilerInternalOptions.IncludeScriptType));
        }

        private static bool IsEnumMember(ISymbol symbol)
        {
            return symbol != null
                && symbol.Kind == SymbolKind.Field
                && symbol.ContainingType != null
                && symbol.ContainingType.TypeKind == TypeKind.Enum
                && symbol.Name != WellKnownMemberNames.EnumBackingFieldName;
        }

        protected override void AddLiteralValue(SpecialType type, object value)
        {
            throw new NotImplementedException();
        }

        protected override void AddExplicitlyCastedLiteralValue(INamedTypeSymbol namedType, SpecialType type, object value)
        {
            throw new NotImplementedException();
        }

        protected override void AddBitwiseOr()
        {
            throw new NotImplementedException();
        }

        protected override bool ShouldRestrictMinimallyQualifyLookupToNamespacesAndTypes()
        {
            throw new NotImplementedException();
        }
    }
}

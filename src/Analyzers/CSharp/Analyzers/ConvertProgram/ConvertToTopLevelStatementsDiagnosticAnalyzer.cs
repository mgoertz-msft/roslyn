﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.Analyzers.ConvertProgram;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Shared.Extensions;

namespace Microsoft.CodeAnalysis.CSharp.TopLevelStatements
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal sealed class ConvertToTopLevelStatementsDiagnosticAnalyzer : AbstractBuiltInCodeStyleDiagnosticAnalyzer
    {
        public ConvertToTopLevelStatementsDiagnosticAnalyzer()
            : base(
                  IDEDiagnosticIds.UseTopLevelStatementsId,
                  EnforceOnBuildValues.UseTopLevelStatements,
                  CSharpCodeStyleOptions.PreferTopLevelStatements,
                  LanguageNames.CSharp,
                  new LocalizableResourceString(nameof(CSharpAnalyzersResources.Convert_to_top_level_statements), CSharpAnalyzersResources.ResourceManager, typeof(CSharpAnalyzersResources)))
        {
        }

        public override DiagnosticAnalyzerCategory GetAnalyzerCategory()
            => DiagnosticAnalyzerCategory.SemanticDocumentAnalysis;

        protected override void InitializeWorker(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(context =>
            {
                // can only suggest moving to top level statement on c# 9 or above.
                if (context.Compilation.LanguageVersion() < LanguageVersion.CSharp9)
                    return;

                context.RegisterSyntaxNodeAction(ProcessCompilationUnit, SyntaxKind.CompilationUnit);
            });
        }

        private void ProcessCompilationUnit(SyntaxNodeAnalysisContext context)
        {
            var options = context.Options;
            var root = (CompilationUnitSyntax)context.Node;

            // Don't want to suggest moving if the user doesn't have a preference for top-level-statements.
            var optionSet = options.GetAnalyzerOptionSet(root.SyntaxTree, context.CancellationToken);
            var option = optionSet.GetOption(CSharpCodeStyleOptions.PreferTopLevelStatements);
            if (!ConvertProgramAnalysis.CanOfferUseTopLevelStatements(option, forAnalyzer: true))
                return;

            var cancellationToken = context.CancellationToken;
            var semanticModel = context.SemanticModel;
            var compilation = semanticModel.Compilation;
            var mainTypeName = compilation.Options.MainTypeName;
            var mainLastTypeName = mainTypeName?.Split('.').Last();

            // Ok, the user does like top level statements.  Check if we can find a suitable hit in this type that
            // indicates we're on the entrypoint of the program.
            foreach (var child in root.DescendantNodes(n => n is CompilationUnitSyntax or NamespaceDeclarationSyntax or ClassDeclarationSyntax))
            {
                if (child is MethodDeclarationSyntax methodDeclaration &&
                    methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword) &&
                    methodDeclaration.TypeParameterList is null &&
                    methodDeclaration.Identifier.ValueText == WellKnownMemberNames.EntryPointMethodName &&
                    methodDeclaration.Parent is TypeDeclarationSyntax containingTypeDeclaration &&
                    methodDeclaration.Body != null)
                {
                    if (mainLastTypeName != null && containingTypeDeclaration.Identifier.ValueText != mainLastTypeName)
                        continue;

                    if (methodDeclaration.ParameterList.Parameters.Count == 1 &&
                        methodDeclaration.ParameterList.Parameters[0].Identifier.ValueText != "args")
                    {
                        continue;
                    }

                    // Have a `static Main` method, and the containing type either matched the type-name the options
                    // specify, or the options specified no type name.  This is a reasonable candidate for the 
                    // program entrypoint.
                    var entryPointMethod = compilation.GetEntryPoint(cancellationToken);
                    if (entryPointMethod == null)
                        continue;

                    var thisMethod = semanticModel.GetDeclaredSymbol(methodDeclaration);
                    if (!entryPointMethod.Equals(thisMethod))
                        continue;

                    // We found the entrypoint.  However, we can only effectively convert this to top-level-statements
                    // if the existing type is amenable to that.
                    if (!TypeCanBeConverted(entryPointMethod.ContainingType, containingTypeDeclaration))
                        return;

                    // Looks good.  Let the user know this type/method can be converted to a top level program.
                    var severity = option.Notification.Severity;
                    context.ReportDiagnostic(DiagnosticHelper.Create(
                        this.Descriptor,
                        ConvertProgramAnalysis.GetUseTopLevelStatementsDiagnosticLocation(
                            methodDeclaration, isHidden: severity.WithDefaultSeverity(DiagnosticSeverity.Hidden) == ReportDiagnostic.Hidden),
                        severity,
                        ImmutableArray.Create(methodDeclaration.GetLocation()),
                        ImmutableDictionary<string, string?>.Empty));
                }
            }
        }

        private static bool TypeCanBeConverted(INamedTypeSymbol containingType, TypeDeclarationSyntax typeDeclaration)
        {
            // Can't convert if our Program type derives from anything special.
            if (containingType.BaseType?.SpecialType != SpecialType.System_Object)
                return false;

            if (containingType.AllInterfaces.Length > 0)
                return false;

            // Too complex to convert many parts to top-level statements.  Just bail on this for now.
            if (containingType.DeclaringSyntaxReferences.Length > 1)
                return false;

            // Too complex to support converting a nested type.
            if (containingType.ContainingType != null)
                return false;

            if (containingType.DeclaredAccessibility != Accessibility.Internal)
                return false;

            // type can't be converted with attributes.
            if (typeDeclaration.AttributeLists.Count > 0)
                return false;

            // can't convert doc comments to top level statements.
            if (typeDeclaration.GetLeadingTrivia().Any(t => t.IsDocComment()))
                return false;

            // All the members of the type need to be private/static.  And we can only have fields or methods. that's to
            // ensure that no one else was calling into this type, and that we can convert everything in the type to
            // either locals or local-functions.

            foreach (var member in typeDeclaration.Members)
            {
                // method can't be converted with attributes.
                if (member.AttributeLists.Count > 0)
                    return false;

                // if not private, can't convert as something may be referencing it.
                if (member.Modifiers.Any(m => m.Kind() is SyntaxKind.PublicKeyword or SyntaxKind.ProtectedKeyword or SyntaxKind.InternalKeyword))
                    return false;

                if (!member.Modifiers.Any(SyntaxKind.StaticKeyword))
                    return false;

                if (member is not FieldDeclarationSyntax and not MethodDeclarationSyntax)
                    return false;

                if (member is MethodDeclarationSyntax methodDeclaration)
                {
                    // if a method, it has to actually have a body so we can convert it to a local function.
                    if (methodDeclaration is { Body: null, ExpressionBody: null })
                        return false;

                    // local functions can't be unsafe
                    if (methodDeclaration.Modifiers.Any(SyntaxKind.UnsafeKeyword))
                        return false;
                }

                // can't convert doc comments to top level statements.
                if (member.GetLeadingTrivia().Any(t => t.IsDocComment()))
                    return false;
            }

            return true;
        }
    }
}

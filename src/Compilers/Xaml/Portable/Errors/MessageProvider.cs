// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Globalization;
using Microsoft.CodeAnalysis.Xaml.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml
{
    internal sealed class MessageProvider : CommonMessageProvider, IObjectWritable
    {
        public static readonly MessageProvider Instance = new MessageProvider();

        static MessageProvider()
        {
            ObjectBinder.RegisterTypeReader(typeof(MessageProvider), r => Instance);
        }

        private MessageProvider()
        {
        }

        void IObjectWritable.WriteTo(ObjectWriter writer)
        {
            // write nothing, always read/deserialized as global Instance
        }

        public override DiagnosticSeverity GetSeverity(int code)
        {
            return ErrorFacts.GetSeverity((ErrorCode)code);
        }

        public override string LoadMessage(int code, CultureInfo language)
        {
            return ErrorFacts.GetMessage((ErrorCode)code, language);
        }

        public override LocalizableString GetMessageFormat(int code)
        {
            return ErrorFacts.GetMessageFormat((ErrorCode)code);
        }

        public override LocalizableString GetDescription(int code)
        {
            return ErrorFacts.GetDescription((ErrorCode)code);
        }

        public override LocalizableString GetTitle(int code)
        {
            return ErrorFacts.GetTitle((ErrorCode)code);
        }

        public override string GetHelpLink(int code)
        {
            return ErrorFacts.GetHelpLink((ErrorCode)code);
        }

        public override string GetCategory(int code)
        {
            return ErrorFacts.GetCategory((ErrorCode)code);
        }

        public override string CodePrefix
        {
            get
            {
                return "XAML";
            }
        }

        // Given a message identifier (e.g., CS0219), severity, warning as error and a culture, 
        // get the entire prefix (e.g., "error CS0219:" for XAML) used on error messages.
        public override string GetMessagePrefix(string id, DiagnosticSeverity severity, bool isWarningAsError, CultureInfo culture)
        {
            return String.Format(culture, "{0} {1}",
                severity == DiagnosticSeverity.Error || isWarningAsError ? "error" : "warning",
                id);
        }

        public override int GetWarningLevel(int code)
        {
            return ErrorFacts.GetWarningLevel((ErrorCode)code);
        }

        public override Type ErrorCodeType
        {
            get { return typeof(ErrorCode); }
        }

        public override Diagnostic CreateDiagnostic(int code, Location location, params object[] args)
        {
            var info = new DiagnosticInfo(Xaml.MessageProvider.Instance, code, args);
            return new XamlDiagnostic(info, location);
        }

        public override Diagnostic CreateDiagnostic(DiagnosticInfo info)
        {
            return new XamlDiagnostic(info, Location.None);
        }

        public override string GetErrorDisplayString(ISymbol symbol)
        {
            // show extra info for assembly if possible such as version, public key token etc.
            if (symbol.Kind == SymbolKind.Assembly || symbol.Kind == SymbolKind.Namespace)
            {
                return symbol.ToString();
            }

            return SymbolDisplay.ToDisplayString(symbol, SymbolDisplayFormat.CSharpShortErrorMessageFormat);
        }

        public override ReportDiagnostic GetDiagnosticReport(DiagnosticInfo diagnosticInfo, CompilationOptions options)
        {
            return ReportDiagnostic.Default;

            // TODO: MGoertz
            //return XamlDiagnosticFilter.GetDiagnosticReport(diagnosticInfo.Severity,
            //                                                  true,
            //                                                  diagnosticInfo.MessageIdentifier,
            //                                                  diagnosticInfo.WarningLevel,
            //                                                  Location.None,
            //                                                  diagnosticInfo.Category,
            //                                                  options.WarningLevel,
            //                                                  options.GeneralDiagnosticOption,
            //                                                  options.SpecificDiagnosticOptions,
            //                                                  out bool hasPragmaSuppression);
        }

        #region NotImplemented

        public override int ERR_FailedToCreateTempFile => throw new NotImplementedException();

        public override int ERR_ExpectedSingleScript => throw new NotImplementedException();

        public override int ERR_OpenResponseFile => throw new NotImplementedException();

        public override int ERR_InvalidPathMap => throw new NotImplementedException();

        public override int FTL_InputFileNameTooLong => throw new NotImplementedException();

        public override int ERR_FileNotFound => throw new NotImplementedException();

        public override int ERR_NoSourceFile => throw new NotImplementedException();

        public override int ERR_CantOpenFileWrite => throw new NotImplementedException();

        public override int ERR_OutputWriteFailed => throw new NotImplementedException();

        public override int WRN_NoConfigNotOnCommandLine => throw new NotImplementedException();

        public override int ERR_BinaryFile => throw new NotImplementedException();

        public override int WRN_UnableToLoadAnalyzer => throw new NotImplementedException();

        public override int INF_UnableToLoadSomeTypesInAnalyzer => throw new NotImplementedException();

        public override int WRN_AnalyzerCannotBeCreated => throw new NotImplementedException();

        public override int WRN_NoAnalyzerInAssembly => throw new NotImplementedException();

        public override int ERR_CantReadRulesetFile => throw new NotImplementedException();

        public override int ERR_CompileCancelled => throw new NotImplementedException();

        public override int ERR_BadCompilationOptionValue => throw new NotImplementedException();

        public override int ERR_MutuallyExclusiveOptions => throw new NotImplementedException();

        public override int ERR_InvalidDebugInformationFormat => throw new NotImplementedException();

        public override int ERR_InvalidFileAlignment => throw new NotImplementedException();

        public override int ERR_InvalidSubsystemVersion => throw new NotImplementedException();

        public override int ERR_InvalidOutputName => throw new NotImplementedException();

        public override int ERR_InvalidInstrumentationKind => throw new NotImplementedException();

        public override int ERR_MetadataFileNotAssembly => throw new NotImplementedException();

        public override int ERR_MetadataFileNotModule => throw new NotImplementedException();

        public override int ERR_InvalidAssemblyMetadata => throw new NotImplementedException();

        public override int ERR_InvalidModuleMetadata => throw new NotImplementedException();

        public override int ERR_ErrorOpeningAssemblyFile => throw new NotImplementedException();

        public override int ERR_ErrorOpeningModuleFile => throw new NotImplementedException();

        public override int ERR_MetadataFileNotFound => throw new NotImplementedException();

        public override int ERR_MetadataReferencesNotSupported => throw new NotImplementedException();

        public override int ERR_LinkedNetmoduleMetadataMustProvideFullPEImage => throw new NotImplementedException();

        public override int ERR_PublicKeyFileFailure => throw new NotImplementedException();

        public override int ERR_PublicKeyContainerFailure => throw new NotImplementedException();

        public override int ERR_OptionMustBeAbsolutePath => throw new NotImplementedException();

        public override int ERR_CantReadResource => throw new NotImplementedException();

        public override int ERR_CantOpenWin32Resource => throw new NotImplementedException();

        public override int ERR_CantOpenWin32Manifest => throw new NotImplementedException();

        public override int ERR_CantOpenWin32Icon => throw new NotImplementedException();

        public override int ERR_BadWin32Resource => throw new NotImplementedException();

        public override int ERR_ErrorBuildingWin32Resource => throw new NotImplementedException();

        public override int ERR_ResourceNotUnique => throw new NotImplementedException();

        public override int ERR_ResourceFileNameNotUnique => throw new NotImplementedException();

        public override int ERR_ResourceInModule => throw new NotImplementedException();

        public override int ERR_PermissionSetAttributeFileReadError => throw new NotImplementedException();

        public override int ERR_EncodinglessSyntaxTree => throw new NotImplementedException();

        public override int WRN_PdbUsingNameTooLong => throw new NotImplementedException();

        public override int WRN_PdbLocalNameTooLong => throw new NotImplementedException();

        public override int ERR_PdbWritingFailed => throw new NotImplementedException();

        public override int ERR_MetadataNameTooLong => throw new NotImplementedException();

        public override int ERR_EncReferenceToAddedMember => throw new NotImplementedException();

        public override int ERR_TooManyUserStrings => throw new NotImplementedException();

        public override int ERR_PeWritingFailure => throw new NotImplementedException();

        public override int ERR_ModuleEmitFailure => throw new NotImplementedException();

        public override int ERR_EncUpdateFailedMissingAttribute => throw new NotImplementedException();

        public override int ERR_BadAssemblyName => throw new NotImplementedException();

        public override void ReportDuplicateMetadataReferenceStrong(DiagnosticBag diagnostics, Location location, MetadataReference reference, AssemblyIdentity identity, MetadataReference equivalentReference, AssemblyIdentity equivalentIdentity)
        {
            throw new NotImplementedException();
        }

        public override void ReportDuplicateMetadataReferenceWeak(DiagnosticBag diagnostics, Location location, MetadataReference reference, AssemblyIdentity identity, MetadataReference equivalentReference, AssemblyIdentity equivalentIdentity)
        {
            throw new NotImplementedException();
        }

        public override void ReportInvalidAttributeArgument(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, AttributeData attribute)
        {
            throw new NotImplementedException();
        }

        public override void ReportInvalidNamedArgument(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int namedArgumentIndex, ITypeSymbol attributeClass, string parameterName)
        {
            throw new NotImplementedException();
        }

        public override void ReportParameterNotValidForType(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int namedArgumentIndex)
        {
            throw new NotImplementedException();
        }

        public override void ReportMarshalUnmanagedTypeNotValidForFields(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, string unmanagedTypeName, AttributeData attribute)
        {
            throw new NotImplementedException();
        }

        public override void ReportMarshalUnmanagedTypeOnlyValidForFields(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, int parameterIndex, string unmanagedTypeName, AttributeData attribute)
        {
            throw new NotImplementedException();
        }

        public override void ReportAttributeParameterRequired(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, string parameterName)
        {
            throw new NotImplementedException();
        }

        public override void ReportAttributeParameterRequired(DiagnosticBag diagnostics, SyntaxNode attributeSyntax, string parameterName1, string parameterName2)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Microsoft.CodeAnalysis.Xaml
{
    internal static partial class ErrorFacts
    {
        private static readonly string s_titleSuffix = "_Title";
        private static readonly string s_descriptionSuffix = "_Description";
        private static readonly Lazy<ImmutableDictionary<ErrorCode, string>> s_helpLinksMap = new Lazy<ImmutableDictionary<ErrorCode, string>>(CreateHelpLinks);
        private static readonly Lazy<ImmutableDictionary<ErrorCode, string>> s_categoriesMap = new Lazy<ImmutableDictionary<ErrorCode, string>>(CreateCategoriesMap);

        private static ImmutableDictionary<ErrorCode, string> CreateHelpLinks()
        {
            var map = new Dictionary<ErrorCode, string>()
            {
                // { ERROR_CODE,    HELP_LINK }
            };

            return map.ToImmutableDictionary();
        }

        private static ImmutableDictionary<ErrorCode, string> CreateCategoriesMap()
        {
            var map = new Dictionary<ErrorCode, string>()
            {
                // { ERROR_CODE,    CATEGORY }
            };

            return map.ToImmutableDictionary();
        }

        internal static DiagnosticSeverity GetSeverity(ErrorCode code)
        {
            // TODO:Mgoertz
            //if (IsWarning(code))
            //{
            //    return DiagnosticSeverity.Warning;
            //}
            //else if (IsInfo(code))
            //{
            //    return DiagnosticSeverity.Info;
            //}
            //else if (IsHidden(code))
            //{
            //    return DiagnosticSeverity.Hidden;
            //}
            //else
            {
                return DiagnosticSeverity.Error;
            }
        }

        /// <remarks>Don't call this during a parse--it loads resources</remarks>
        public static string GetMessage(ErrorCode code, CultureInfo culture)
        {
            string message = ResourceManager.GetString(ConvertError(code) /*code.ToString()*/, culture);
            Debug.Assert(message != null);
            return message;
        }

        public static LocalizableResourceString GetMessageFormat(ErrorCode code)
        {
            return new LocalizableResourceString(code.ToString(), ResourceManager, typeof(ErrorFacts));
        }

        public static LocalizableResourceString GetTitle(ErrorCode code)
        {
            return new LocalizableResourceString(code.ToString() + s_titleSuffix, ResourceManager, typeof(ErrorFacts));
        }

        public static LocalizableResourceString GetDescription(ErrorCode code)
        {
            return new LocalizableResourceString(code.ToString() + s_descriptionSuffix, ResourceManager, typeof(ErrorFacts));
        }

        public static string GetHelpLink(ErrorCode code)
        {
            string helpLink;
            if (s_helpLinksMap.Value.TryGetValue(code, out helpLink))
            {
                return helpLink;
            }

            return string.Empty;
        }

        public static string GetCategory(ErrorCode code)
        {
            string category;
            if (s_categoriesMap.Value.TryGetValue(code, out category))
            {
                return category;
            }

            return Diagnostic.CompilerDiagnosticCategory;
        }

        ///// <remarks>Don't call this during a parse--it loads resources</remarks>
        //public static string GetMessage(XmlParseErrorCode id, CultureInfo culture)
        //{
        //    return ResourceManager.GetString(id.ToString(), culture);
        //}

        private static System.Resources.ResourceManager s_resourceManager;
        private static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (s_resourceManager == null)
                {
                    s_resourceManager = new System.Resources.ResourceManager("Microsoft.CodeAnalysis.Xaml.XamlResources", typeof(ErrorCode).GetTypeInfo().Assembly);
                }

                return s_resourceManager;
            }
        }

        internal static int GetWarningLevel(ErrorCode code)
        {
            //if (IsInfo(code) || IsHidden(code))
            //{
            //    // Info and hidden diagnostics have least warning level.
            //    return Diagnostic.HighestValidWarningLevel;
            //}
            return 0;
        }

        static string ConvertError(ErrorCode error)//, object[] args)
        {
            string msg = null;
            switch (error)
            {
                case ErrorCode.UnterminatedString: msg = "Error_UnterminatedString"; break;
                case ErrorCode.UnescapedOpenTag: msg = "Error_UnescapedOpenTag"; break;
                case ErrorCode.UnterminatedCharacterData: msg = "Error_UnterminatedCharacterData"; break;
                case ErrorCode.UnterminatedComment: msg = "Error_UnterminatedComment"; break;
                case ErrorCode.CommentedEndedWithDoubleHyphenBangGreaterThan: msg = "Error_CommentedEndedWithDoubleHyphenBangGreaterThan"; break;
                case ErrorCode.CommentWithDoubleHyphen: msg = "Error_CommentWithDoubleHyphen"; break;
                case ErrorCode.NoSuchNamedEntity: msg = "Error_NoSuchNamedEntity"; break;
                case ErrorCode.MissingSemicolonInEntity: msg = "Error_MissingSemicolonInEntity"; break;
                case ErrorCode.BadDecimalDigit: msg = "Error_BadDecimalDigit"; break;
                case ErrorCode.BadHexDigit: msg = "Error_BadHexDigit"; break;
                case ErrorCode.EntityOverflow: msg = "Error_EntityOverflow"; break;
                case ErrorCode.ExpectedDifferentToken: msg = "Error_ExpectedDifferentToken"; break;
                case ErrorCode.UnterminatedProcessingInstruction: msg = "Error_UnterminatedProcessingInstruction"; break;
                case ErrorCode.UnterminatedXmlText: msg = "Error_UnterminatedXmlText"; break;

                case ErrorCode.AmbiguousTypeReference: msg = "Error_AmbiguousTypeReference"; break;
                case ErrorCode.UnableToLoadAssemlby: msg = "Error_UnableToLoadAssemlby"; break;
                case ErrorCode.InvalidXmlnsDefinitionAttribute: msg = "Error_InvalidXmlnsDefinitionAttribute"; break;
                case ErrorCode.EmptyXmlnsDeclaration: msg = "Error_EmptyXmlnsDeclaration"; break;

                case ErrorCode.UnexpectedToken: msg = "Error_UnexpectedToken"; break;
                case ErrorCode.IllegalWhitespaceInIdentifier: msg = "Error_IllegalWhitespaceInIdentifier"; break;
                case ErrorCode.ExpectedIdentifier: msg = "Error_ExpectedIdentifier"; break;
                case ErrorCode.ExpectedString: msg = "Error_ExpectedString"; break;
                case ErrorCode.ClosingTagMismatch: msg = "Error_ClosingTagMismatch"; break;
                case ErrorCode.MaxDepthReached: msg = "Error_MaxDepthReached"; break;

                case ErrorCode.ExpectedWhitespace: msg = "Error_ExpectedWhitespace"; break;
                case ErrorCode.EmptyDocument: msg = "Error_EmptyDocument"; break;

                default: Debug.Fail("Invalid error value"); break;
            }
            return msg;// string.Format(CultureInfo.CurrentCulture, msg, args);
        }
    }
}

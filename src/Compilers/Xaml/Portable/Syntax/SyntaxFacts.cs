// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Xaml.Syntax;
using Roslyn.Utilities;
using static Microsoft.CodeAnalysis.Xaml.SyntaxKind;

namespace Microsoft.CodeAnalysis.Xaml
{
    public static partial class SyntaxFacts
    {
        public static string GetText(Accessibility accessibility)
        {
            switch (accessibility)
            {
                case Accessibility.NotApplicable:
                    return string.Empty;
                case Accessibility.Private:
                    return SyntaxFacts.GetText(PrivateKeyword);
                case Accessibility.ProtectedAndInternal:
                    // TODO: XAML doesn't have a representation for this.
                    // For now, use Reflector's representation.
                    return SyntaxFacts.GetText(InternalKeyword) + " " + SyntaxFacts.GetText(ProtectedKeyword);
                case Accessibility.Internal:
                    return SyntaxFacts.GetText(InternalKeyword);
                case Accessibility.Protected:
                    return SyntaxFacts.GetText(ProtectedKeyword);
                case Accessibility.ProtectedOrInternal:
                    return SyntaxFacts.GetText(ProtectedKeyword) + " " + SyntaxFacts.GetText(InternalKeyword);
                case Accessibility.Public:
                    return SyntaxFacts.GetText(PublicKeyword);
                default:
                    throw ExceptionUtilities.UnexpectedValue(accessibility);
            }
        }
    }
}
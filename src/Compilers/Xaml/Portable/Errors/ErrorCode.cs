// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xaml
{
    internal enum ErrorCode
    {
        // Scanner errors
        ScannerErrorsBegin = 100,
        UnterminatedString = 101,
        UnescapedOpenTag = 102,
        UnterminatedCharacterData = 103,
        UnterminatedComment = 104,
        CommentedEndedWithDoubleHyphenBangGreaterThan = 105,
        CommentWithDoubleHyphen = 106,
        NoSuchNamedEntity = 107,
        MissingSemicolonInEntity = 108,
        BadDecimalDigit = 109,
        BadHexDigit = 110,
        EntityOverflow = 111,
        ExpectedDifferentToken = 112,
        UnterminatedProcessingInstruction = 113,
        UnterminatedXmlText = 114,
        ScannerErrorsEnd = 199,

        // Symboltable Errors
        SymboltableErrorsBegin = 200,
        AmbiguousTypeReference = 201,
        UnableToLoadAssemlby = 202,
        InvalidXmlnsDefinitionAttribute = 203,
        EmptyXmlnsDeclaration = 204,
        SymboltableErrorsEnd = 299,

        // Parsing errors
        ParsingErrorsBegin = 300,
        UnexpectedToken = 301,
        IllegalWhitespaceInIdentifier = 302,
        ExpectedIdentifier = 303,
        ExpectedString = 304,
        ClosingTagMismatch = 305,
        MaxDepthReached = 306,
        ExpectedWhitespace = 307,
        EmptyDocument = 308,
        ParsingErrorsEnd = 399,

        LastError
    }
}

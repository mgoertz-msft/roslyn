// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xaml
{
    // DO NOT CHANGE NUMBERS ASSIGNED TO EXISTING KINDS OR YOU WILL BREAK BINARY COMPATIBILITY
    public enum SyntaxKind : ushort
    {
        None = 0,
        List = GreenNode.ListKind,

        XamlFirst = 9000,	// =

        Assign = 9001,	// =
        Colon, // :
        Comma, // ,
        Comment, // <!-- .... -->
        CharacterData, // <![CDATA[ chars ]]>
        Dot, // .
        EndOfLine, // [cr] lf
        EndOfProcessingInstruction,	// ?>
        EndOfTag, // >
        EndOfSimpleTag,	// />
        Identifier,
        IllegalCharacter,
        LeftBrace, // {
        LeftBracket, // [
        LeftParenthesis, // (
        LiteralContentString,
        Plus, //+
        ProcessingInstruction, //<? processing instruction ?>
        RightBrace,	//}
        RightBracket, // ]
        RightParenthesis, // )
        Star, // *
        StartOfClosingTag, // </
        StartOfTag,	// <
        StringLiteral, //" ... "
        Whitespace,	//
        StartOfRegion,// <!--#region %L-->
        EndOfRegion,// <!--#endregion-->

        // punctuation
        TildeToken = 9193,
        ExclamationToken = 9194,
        DollarToken = 9195,
        PercentToken = 9196,
        CaretToken = 9197,
        AmpersandToken = 9198,
        AsteriskToken = Star,
        OpenParenToken = LeftParenthesis,
        CloseParenToken = RightParenthesis,
        MinusToken = 9202,
        PlusToken = Plus,
        EqualsToken = Assign,
        OpenBraceToken = LeftBrace,
        CloseBraceToken = RightBrace,
        OpenBracketToken = LeftBracket,
        CloseBracketToken = RightBracket,
        BarToken = 9209,
        BackslashToken = 9210,
        ColonToken = Colon,
        SemicolonToken = 9212,
        DoubleQuoteToken = 9213,
        SingleQuoteToken = 9214,
        LessThanToken = StartOfTag,
        CommaToken = Comma,
        GreaterThanToken = EndOfTag,
        DotToken = Dot,
        QuestionToken = 9219,
        HashToken = 9220,
        SlashToken = 9221,

        // additional Xaml tokens
        SlashGreaterThanToken = EndOfSimpleTag, // Xaml empty element end
        LessThanSlashToken = StartOfClosingTag, // element end tag start token
        XamlCommentStartToken = 9234, // <!--
        XamlCommentEndToken = 9235, // -->
        XamlCDataStartToken = 9236, // <![CDATA[
        XamlCDataEndToken = 9237, // ]]>
        XamlProcessingInstructionStartToken = 9238, // <?
        XamlProcessingInstructionEndToken = 9239, // ?>


        // Keywords
        NullKeyword = 9322,
        TrueKeyword = 9323,
        FalseKeyword = 9324,

        PublicKeyword = 9343,
        PrivateKeyword = 9344,
        InternalKeyword = 9345,
        ProtectedKeyword = 9346,

        //// additional preprocessor keywords
        //RegionKeyword = 9469,
        //EndRegionKeyword = 9470,
        NamespaceKeyword = 9372,

        // Other
        UnderscoreToken = 9491,
        EndOfDocumentationCommentToken = 9495,

        EndOfFile = 9499,
        EndOfFileToken = EndOfFile, //NB: this is assumed to be the last textless token

        // tokens with text
        BadToken = 9507,
        IdentifierToken = Identifier,
        NumericLiteralToken = 9509,
        CharacterLiteralToken = 9510,
        StringLiteralToken = 9511,
        XamlEntityLiteralToken = 9512,  // &lt; &gt; &quot; &amp; &apos; or &name; or &#nnnn; or &#xhhhh;
        XamlTextLiteralToken = 9513,    // Xaml text node text
        XamlTextLiteralNewLineToken = 9514,

        // trivia
        EndOfLineTrivia = 9539,
        WhitespaceTrivia = 9540,
        SingleLineCommentTrivia = 9541,
        MultiLineCommentTrivia = 9542,
        DocumentationCommentExteriorTrivia = 9543,
        SingleLineDocumentationCommentTrivia = 9544,
        MultiLineDocumentationCommentTrivia = 9545,
        DisabledTextTrivia = 9546,
        RegionDirectiveTrivia = 9552,
        EndRegionDirectiveTrivia = 9553,
        SkippedTokensTrivia = 9563,

        // Xaml nodes (for Xaml doc comment structure)
        XamlElement = 9574,
        XamlElementStartTag = 9575,
        XamlElementEndTag = 9576,
        XamlEmptyElement = 9577,
        XamlTextAttribute = 9578,
        XamlCrefAttribute = 9579,
        XamlNameAttribute = 9580,
        XamlName = 9581,
        XamlPrefix = 9582,
        XamlText = 9583,
        XamlCDataSection = 9584,
        XamlComment = 9585,
        XamlProcessingInstruction = 9586,

        //// documentation comment nodes (structure inside DocumentationCommentTrivia)
        //TypeCref = 9597,
        //QualifiedCref = 9598,
        //NameMemberCref = 9599,
        //IndexerMemberCref = 9600,
        //OperatorMemberCref = 9601,
        //ConversionOperatorMemberCref = 9602,
        //CrefParameterList = 9603,
        //CrefBracketedParameterList = 9604,
        //CrefParameter = 9605,

        // names & type-names
        IdentifierName = 9616,
        QualifiedName = 9617,
        GenericName = 9618,
        TypeArgumentList = 9619,
        AliasQualifiedName = 9620,
        PredefinedType = 9621,
        ArrayType = 9622,
        ArrayRankSpecifier = 9623,
        PointerType = 9624,
        NullableType = 9625,
        OmittedTypeArgument = 9626,


        //// primary expression
        //NumericLiteralExpression = 9749,
        //StringLiteralExpression = 9750,
        //CharacterLiteralExpression = 9751,
        //TrueLiteralExpression = 9752,
        //FalseLiteralExpression = 9753,
        //NullLiteralExpression = 9754,

        // declarations
        XamlBody = 9840,

        //// attributes
        //AttributeList = 9847,
        //AttributeTargetSpecifier = 9848,
        //Attribute = 9849,
        //AttributeArgumentList = 9850,
        //AttributeArgument = 9851,
        //NameEquals = 9852,

        XamlLast = 9999,
    }
}

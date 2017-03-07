// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{
    internal partial class SyntaxParser
    {
        protected struct ResetPoint
        {
            internal readonly int ResetCount;
            internal readonly int Position;
            internal readonly GreenNode PrevTokenTrailingTrivia;

            internal ResetPoint(int resetCount, int position, GreenNode prevTokenTrailingTrivia)
            {
                this.ResetCount = resetCount;
                this.Position = position;
                this.PrevTokenTrailingTrivia = prevTokenTrailingTrivia;
            }
        }
    }
}
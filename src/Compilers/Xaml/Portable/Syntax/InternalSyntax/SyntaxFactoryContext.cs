// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{
    /// <summary>
    /// Because syntax nodes need to be constructed with context information - to allow us to 
    /// determine whether or not they can be reused during incremental parsing - the syntax
    /// factory needs a view of some internal parser state.
    /// </summary>
    /// <remarks>
    /// Read-only outside SyntaxParser (not enforced for perf reasons).
    /// Reference type so that the factory stays up-to-date.
    /// </remarks>
    internal class SyntaxFactoryContext
    {
    }
}
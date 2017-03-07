// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Syntax.InternalSyntax;

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{
    internal static class XamlSyntaxNodeCache
    {
        internal static GreenNode TryGetNode(int kind, GreenNode child1, SyntaxFactoryContext context, out int hash)
        {
            return SyntaxNodeCache.TryGetNode(kind, child1, GetNodeFlags(context), out hash);
        }

        internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, SyntaxFactoryContext context, out int hash)
        {
            return SyntaxNodeCache.TryGetNode(kind, child1, child2, GetNodeFlags(context), out hash);
        }

        internal static GreenNode TryGetNode(int kind, GreenNode child1, GreenNode child2, GreenNode child3, SyntaxFactoryContext context, out int hash)
        {
            return SyntaxNodeCache.TryGetNode(kind, child1, child2, child3, GetNodeFlags(context), out hash);
        }

        private static GreenNode.NodeFlags GetNodeFlags(SyntaxFactoryContext context)
        {
            var flags = SyntaxNodeCache.GetDefaultNodeFlags();

            return flags;
        }
    }
}
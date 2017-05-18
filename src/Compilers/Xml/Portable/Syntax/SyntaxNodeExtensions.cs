// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.CodeAnalysis.Xml.Syntax;

namespace Microsoft.CodeAnalysis.Xml
{
    internal static class SyntaxNodeExtensions
    {
        public static TNode WithAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : XmlSyntaxNode
        {
            return (TNode)node.Green.SetAnnotations(annotations).CreateRed();
        }
    }
}

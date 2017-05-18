// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xml.Syntax.InternalSyntax
{
    internal struct BlendedNode
    {
        internal readonly Xml.XmlSyntaxNode Node;
        internal readonly SyntaxToken Token;
        internal readonly Blender Blender;

        internal BlendedNode(Xml.XmlSyntaxNode node, SyntaxToken token, Blender blender)
        {
            this.Node = node;
            this.Token = token;
            this.Blender = blender;
        }
    }
}

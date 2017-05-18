// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Xml.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Xml
{
    public partial class XmlSyntaxTree
    {
        internal sealed class DummySyntaxTree : XmlSyntaxTree
        {
            XmlSyntaxNode _node;

            public DummySyntaxTree()
            {
            }

            public override string ToString()
            {
                return string.Empty;
            }

            public override SourceText GetText(CancellationToken cancellationToken)
            {
                return SourceText.From(string.Empty, Encoding.UTF8);
            }

            public override bool TryGetText(out SourceText text)
            {
                text = SourceText.From(string.Empty, Encoding.UTF8);
                return true;
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            public override int Length
            {
                get { return 0; }
            }

            public override XmlParseOptions Options
            {
                get { return XmlParseOptions.Default; }
            }

            public override string FilePath
            {
                get { return string.Empty; }
            }

            public override SyntaxReference GetReference(SyntaxNode node)
            {
                return new SimpleSyntaxReference(node);
            }

            public override XmlSyntaxNode GetRoot(CancellationToken cancellationToken)
            {
                return null;
            }

            public override bool TryGetRoot(out XmlSyntaxNode root)
            {
                root = _node;
                return true;
            }

            public override bool HasCompilationUnitRoot
            {
                get { return false; }
            }

            public override FileLinePositionSpan GetLineSpan(TextSpan span, CancellationToken cancellationToken = default(CancellationToken))
            {
                return default(FileLinePositionSpan);
            }

            public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
            {
                return SyntaxFactory.SyntaxTree(root, options: options, path: FilePath, encoding: null);
            }

            public override SyntaxTree WithFilePath(string path)
            {
                return SyntaxFactory.SyntaxTree(_node, options: this.Options, path: path, encoding: null);
            }
        }
    }
}

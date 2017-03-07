// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Xaml
{
    public partial class XamlSyntaxTree
    {
        private class ParsedSyntaxTree : XamlSyntaxTree
        {
            private readonly XamlParseOptions _options;
            private readonly string _path;
            private readonly XamlSyntaxNode _root;
            private readonly Encoding _encodingOpt;
            private readonly SourceHashAlgorithm _checksumAlgorithm;
            private SourceText _lazyText;

            internal ParsedSyntaxTree(SourceText textOpt, Encoding encodingOpt, SourceHashAlgorithm checksumAlgorithm, string path, XamlParseOptions options, XamlSyntaxNode root, bool cloneRoot = true)
            {
                Debug.Assert(root != null);
                Debug.Assert(options != null);
                Debug.Assert(textOpt == null || textOpt.Encoding == encodingOpt && textOpt.ChecksumAlgorithm == checksumAlgorithm);

                _lazyText = textOpt;
                _encodingOpt = encodingOpt ?? textOpt?.Encoding;
                _checksumAlgorithm = checksumAlgorithm;
                _options = options;
                _path = path ?? string.Empty;
                _root = cloneRoot ? this.CloneNodeAsRoot(root) : root;
            }

            public override string FilePath
            {
                get { return _path; }
            }

            public override SourceText GetText(CancellationToken cancellationToken)
            {
                if (_lazyText == null)
                {
                    Interlocked.CompareExchange(ref _lazyText, this.GetRoot(cancellationToken).GetText(_encodingOpt, _checksumAlgorithm), null);
                }

                return _lazyText;
            }

            public override bool TryGetText(out SourceText text)
            {
                text = _lazyText;
                return text != null;
            }

            public override Encoding Encoding
            {
                get { return _encodingOpt; }
            }

            public override int Length
            {
                get { return _root.FullSpan.Length; }
            }

            public override XamlSyntaxNode GetRoot(CancellationToken cancellationToken)
            {
                return _root;
            }

            public override bool TryGetRoot(out XamlSyntaxNode root)
            {
                root = _root;
                return true;
            }

            public override bool HasCompilationUnitRoot
            {
                get
                {
                    return false;
                }
            }

            public override XamlParseOptions Options
            {
                get
                {
                    return _options;
                }
            }

            public override SyntaxReference GetReference(SyntaxNode node)
            {
                return new SimpleSyntaxReference(node);
            }

            public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
            {
                if (ReferenceEquals(_root, root) && ReferenceEquals(_options, options))
                {
                    return this;
                }

                return new ParsedSyntaxTree(
                    null,
                    _encodingOpt,
                    _checksumAlgorithm,
                    _path,
                    (XamlParseOptions)options,
                    (XamlSyntaxNode)root);
            }

            public override SyntaxTree WithFilePath(string path)
            {
                if (_path == path)
                {
                    return this;
                }

                return new ParsedSyntaxTree(
                    _lazyText,
                    _encodingOpt,
                    _checksumAlgorithm,
                    path,
                    _options,
                    _root);
            }
        }
    }
}

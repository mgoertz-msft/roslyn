// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Xaml.Syntax;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml
{
    internal partial class XamlSyntaxTreeFactoryServiceFactory
    {
        // internal for testing
        internal partial class XamlSyntaxTreeFactoryService
        {
            /// <summary>
            /// Represents a syntax tree that only has a weak reference to its 
            /// underlying data.  This way it can be passed around without forcing
            /// the underlying full tree to stay alive.  Think of it more as a 
            /// key that can be used to identify a tree rather than the tree itself.
            /// </summary>
            internal sealed class RecoverableSyntaxTree : XamlSyntaxTree, IRecoverableSyntaxTree<XamlBodySyntax>, ICachedObjectOwner
            {
                private readonly RecoverableSyntaxRoot<XamlBodySyntax> _recoverableRoot;
                private readonly SyntaxTreeInfo _info;
                private readonly IProjectCacheHostService _projectCacheService;
                private readonly ProjectId _cacheKey;

                object ICachedObjectOwner.CachedObject { get; set; }

                private RecoverableSyntaxTree(AbstractSyntaxTreeFactoryService service, ProjectId cacheKey, XamlBodySyntax root, SyntaxTreeInfo info)
                {
                    _recoverableRoot = new RecoverableSyntaxRoot<XamlBodySyntax>(service, root, this);
                    _info = info;
                    _projectCacheService = service.LanguageServices.WorkspaceServices.GetService<IProjectCacheHostService>();
                    _cacheKey = cacheKey;
                }

                private RecoverableSyntaxTree(RecoverableSyntaxTree original, SyntaxTreeInfo info)
                {
                    _recoverableRoot = original._recoverableRoot.WithSyntaxTree(this);
                    _info = info;
                    _projectCacheService = original._projectCacheService;
                    _cacheKey = original._cacheKey;
                }

                internal static SyntaxTree CreateRecoverableTree(AbstractSyntaxTreeFactoryService service, ProjectId cacheKey, string filePath, ParseOptions options, ValueSource<TextAndVersion> text, Encoding encoding, XamlBodySyntax root)
                {
                    return new RecoverableSyntaxTree(service, cacheKey, root, new SyntaxTreeInfo(filePath, options, text, encoding, root.FullSpan.Length));
                }

                public override string FilePath
                {
                    get { return _info.FilePath; }
                }

                public override XamlParseOptions Options
                {
                    get { return (XamlParseOptions)_info.Options; }
                }

                public override int Length
                {
                    get { return _info.Length; }
                }

                public override bool TryGetText(out SourceText text)
                {
                    return _info.TryGetText(out text);
                }

                public override SourceText GetText(CancellationToken cancellationToken)
                {
                    return _info.TextSource.GetValue(cancellationToken).Text;
                }

                public override Task<SourceText> GetTextAsync(CancellationToken cancellationToken)
                {
                    return _info.GetTextAsync(cancellationToken);
                }

                public override Encoding Encoding
                {
                    get { return _info.Encoding; }
                }

                private XamlBodySyntax CacheRootNode(XamlBodySyntax node)
                {
                    return _projectCacheService.CacheObjectIfCachingEnabledForKey(_cacheKey, this, node);
                }

                public override bool TryGetRoot(out XamlSyntaxNode root)
                {
                    bool status = _recoverableRoot.TryGetValue(out var node);
                    root = node;
                    CacheRootNode(node);
                    return status;
                }

                public override XamlSyntaxNode GetRoot(CancellationToken cancellationToken = default(CancellationToken))
                {
                    return CacheRootNode(_recoverableRoot.GetValue(cancellationToken));
                }

                public override async Task<XamlSyntaxNode> GetRootAsync(CancellationToken cancellationToken)
                {
                    return CacheRootNode(await _recoverableRoot.GetValueAsync(cancellationToken).ConfigureAwait(false));
                }

                public override bool HasCompilationUnitRoot
                {
                    get { return true; }
                }

                public override SyntaxReference GetReference(SyntaxNode node)
                {
                    if (node != null)
                    {
                        // many people will take references to nodes in this tree.  
                        // We don't actually want those references to keep the tree alive.
                        if (node.Span.Length == 0)
                        {
                            return new PathSyntaxReference(node);
                        }
                        else
                        {
                            return new PositionalSyntaxReference(node);
                        }
                    }
                    else
                    {
                        return new NullSyntaxReference(this);
                    }
                }

                XamlBodySyntax IRecoverableSyntaxTree<XamlBodySyntax>.CloneNodeAsRoot(XamlBodySyntax root)
                {
                    return CloneNodeAsRoot(root);
                }

                public override SyntaxTree WithRootAndOptions(SyntaxNode root, ParseOptions options)
                {
                    if (ReferenceEquals(_info.Options, options) && this.TryGetRoot(out var oldRoot) && ReferenceEquals(root, oldRoot))
                    {
                        return this;
                    }

                    return Create((XamlSyntaxNode)root, this.Options, _info.FilePath);
                }

                public override SyntaxTree WithFilePath(string path)
                {
                    if (path == this.FilePath)
                    {
                        return this;
                    }

                    return new RecoverableSyntaxTree(this, _info.WithFilePath(path));
                }
            }
        }
    }
}

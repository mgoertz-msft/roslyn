// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis.Xaml.Syntax;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Text;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml
{
    [ExportLanguageServiceFactory(typeof(ISyntaxTreeFactoryService), LanguageNames.Xaml), Shared]
    internal partial class XamlSyntaxTreeFactoryServiceFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices provider)
        {
            return new XamlSyntaxTreeFactoryService(provider);
        }

        internal partial class XamlSyntaxTreeFactoryService : AbstractSyntaxTreeFactoryService
        {
            public XamlSyntaxTreeFactoryService(HostLanguageServices languageServices) : base(languageServices)
            {
            }

            public override ParseOptions GetDefaultParseOptions()
            {
                return XamlParseOptions.Default;
            }

            public override SyntaxTree CreateSyntaxTree(string fileName, ParseOptions options, Encoding encoding, SyntaxNode root)
            {
                options = options ?? GetDefaultParseOptions();
                return SyntaxFactory.SyntaxTree(root, options, fileName, encoding);
            }

            public override SyntaxTree ParseSyntaxTree(string fileName, ParseOptions options, SourceText text, CancellationToken cancellationToken)
            {
                options = options ?? GetDefaultParseOptions();
                return SyntaxFactory.ParseSyntaxTree(text, options, fileName, cancellationToken: cancellationToken);
            }

            public override SyntaxNode DeserializeNodeFrom(Stream stream, CancellationToken cancellationToken)
                => XamlSyntaxNode.DeserializeFrom(stream, cancellationToken);

            public override bool CanCreateRecoverableTree(SyntaxNode root)
            {
                var body = root as XamlBodySyntax;
                return base.CanCreateRecoverableTree(root) && body != null;
            }

            public override SyntaxTree CreateRecoverableTree(ProjectId cacheKey, string filePath, ParseOptions options, ValueSource<TextAndVersion> text, Encoding encoding, SyntaxNode root)
            {
                System.Diagnostics.Debug.Assert(CanCreateRecoverableTree(root));
                return RecoverableSyntaxTree.CreateRecoverableTree(this, cacheKey, filePath, options ?? GetDefaultParseOptions(), text, encoding, (XamlBodySyntax)root);
            }
        }
    }
}

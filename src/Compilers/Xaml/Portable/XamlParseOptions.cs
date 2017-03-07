// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xaml
{
    /// <summary>
    /// This class stores several source parsing related options and offers access to their values.
    /// </summary>
    public sealed class XamlParseOptions : ParseOptions, IEquatable<XamlParseOptions>
    {
        /// <summary>
        /// The default parse options.
        /// </summary>
        public static XamlParseOptions Default { get; } = new XamlParseOptions();

        private ImmutableDictionary<string, string> _features;

        public XamlParseOptions(
            DocumentationMode documentationMode = DocumentationMode.Parse,
            SourceCodeKind kind = SourceCodeKind.Regular)
            : this(documentationMode, 
                  kind, 
                  ImmutableDictionary<string, string>.Empty)
        {
            if (!kind.IsValid())
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }
        }

        internal XamlParseOptions(
            DocumentationMode documentationMode,
            SourceCodeKind kind,
            ImmutableDictionary<string, string> features)
            : base(kind, documentationMode)
        {
            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            _features = features;
        }

        private XamlParseOptions(XamlParseOptions other) : this(
            documentationMode: other.DocumentationMode,
            kind: other.Kind,
            features: other.Features.ToImmutableDictionary())
        {
        }

        public override string Language => LanguageNames.Xaml;

        public new XamlParseOptions WithKind(SourceCodeKind kind)
        {
            if (kind == this.Kind)
            {
                return this;
            }

            if (!kind.IsValid())
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            return new XamlParseOptions(this) { Kind = kind };
        }

        public new XamlParseOptions WithDocumentationMode(DocumentationMode documentationMode)
        {
            if (documentationMode == this.DocumentationMode)
            {
                return this;
            }

            if (!documentationMode.IsValid())
            {
                throw new ArgumentOutOfRangeException(nameof(documentationMode));
            }

            return new XamlParseOptions(this) { DocumentationMode = documentationMode };
        }

        public override ParseOptions CommonWithKind(SourceCodeKind kind)
        {
            return WithKind(kind);
        }

        protected override ParseOptions CommonWithDocumentationMode(DocumentationMode documentationMode)
        {
            return WithDocumentationMode(documentationMode);
        }

        protected override ParseOptions CommonWithFeatures(IEnumerable<KeyValuePair<string, string>> features)
        {
            return WithFeatures(features);
        }

        /// <summary>
        /// Enable some experimental language features for testing.
        /// </summary>
        public new XamlParseOptions WithFeatures(IEnumerable<KeyValuePair<string, string>> features)
        {
            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            return new XamlParseOptions(this) { _features = features.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase) };
        }

        public override IReadOnlyDictionary<string, string> Features
        {
            get
            {
                return _features;
            }
        }

        public override IEnumerable<string> PreprocessorSymbolNames => Enumerable.Empty<string>();

        public override bool Equals(object obj)
        {
            return this.Equals(obj as XamlParseOptions);
        }

        public bool Equals(XamlParseOptions other)
        {
            if (object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (!base.EqualsHelper(other))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return
                Hash.Combine(base.GetHashCodeHelper(), 0);
        }
    }
}

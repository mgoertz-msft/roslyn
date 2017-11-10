// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.Xml
{
    /// <summary>
    /// This class stores several source parsing related options and offers access to their values.
    /// </summary>
    public sealed class XmlParseOptions : ParseOptions, IEquatable<XmlParseOptions>
    {
        /// <summary>
        /// The default parse options.
        /// </summary>
        public static XmlParseOptions Default { get; } = new XmlParseOptions();

        private ImmutableDictionary<string, string> _features;

        public XmlParseOptions(
            DocumentationMode documentationMode = DocumentationMode.Parse,
            SourceCodeKind kind = SourceCodeKind.Regular)
            : this(documentationMode, 
                  kind, 
                  ImmutableDictionary<string, string>.Empty)
        {
        }

        internal XmlParseOptions(
            DocumentationMode documentationMode,
            SourceCodeKind kind,
            IReadOnlyDictionary<string, string> features)
            : base(kind, documentationMode)
        {
            _features = features?.ToImmutableDictionary() ?? ImmutableDictionary<string, string>.Empty;
        }

        private XmlParseOptions(XmlParseOptions other) : this(
            documentationMode: other.DocumentationMode,
            kind: other.Kind,
            features: other.Features)
        {
        }

        public override string Language => LanguageNames.Xml;

        public new XmlParseOptions WithKind(SourceCodeKind kind)
        {
            if (kind == this.Kind)
            {
                return this;
            }

            if (!kind.IsValid())
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }

            return new XmlParseOptions(this) { Kind = kind };
        }

        public new XmlParseOptions WithDocumentationMode(DocumentationMode documentationMode)
        {
            if (documentationMode == this.DocumentationMode)
            {
                return this;
            }

            return new XmlParseOptions(this) { DocumentationMode = documentationMode };
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
        public new XmlParseOptions WithFeatures(IEnumerable<KeyValuePair<string, string>> features)
        {
            ImmutableDictionary<string, string> dictionary =
                features?.ToImmutableDictionary(StringComparer.OrdinalIgnoreCase)
                ?? ImmutableDictionary<string, string>.Empty;

            return new XmlParseOptions(this) { _features = dictionary };
        }

        public override IReadOnlyDictionary<string, string> Features
        {
            get
            {
                return _features;
            }
        }

        internal override void ValidateOptions(ArrayBuilder<Diagnostic> builder)
        {
            ValidateOptions(builder, MessageProvider.Instance);
        }

        public override IEnumerable<string> PreprocessorSymbolNames => Enumerable.Empty<string>();

        public override bool Equals(object obj)
        {
            return this.Equals(obj as XmlParseOptions);
        }

        public bool Equals(XmlParseOptions other)
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

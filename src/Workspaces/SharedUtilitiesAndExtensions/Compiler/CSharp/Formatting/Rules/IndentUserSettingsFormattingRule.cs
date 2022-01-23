﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting.Rules;
using Microsoft.CodeAnalysis.Options;

namespace Microsoft.CodeAnalysis.CSharp.Formatting
{
    internal sealed class IndentUserSettingsFormattingRule : BaseFormattingRule
    {
        private readonly CachedOptions _options;

        public IndentUserSettingsFormattingRule()
            : this(new CachedOptions(null))
        {
        }

        private IndentUserSettingsFormattingRule(CachedOptions options)
        {
            _options = options;
        }

        public override AbstractFormattingRule WithOptions(AnalyzerConfigOptions options)
        {
            var cachedOptions = new CachedOptions(options);

            if (cachedOptions == _options)
            {
                return this;
            }

            return new IndentUserSettingsFormattingRule(cachedOptions);
        }

        public override void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNode node, in NextIndentBlockOperationAction nextOperation)
        {
            nextOperation.Invoke();

            var bracePair = node.GetBracePair();

            // don't put block indentation operation if the block only contains lambda expression body block
            if (node.IsLambdaBodyBlock() || !bracePair.IsValidBracePair())
            {
                return;
            }

            if (_options.IndentBraces)
            {
                AddIndentBlockOperation(list, bracePair.Item1, bracePair.Item1, bracePair.Item1.Span);
                AddIndentBlockOperation(list, bracePair.Item2, bracePair.Item2, bracePair.Item2.Span);
            }
        }

        private readonly record struct CachedOptions(
            bool IndentBraces
            )
        {
            public CachedOptions(AnalyzerConfigOptions? options) : this(
                IndentBraces: GetOptionOrDefault(options, CSharpFormattingOptions2.IndentBraces)
                )
            {
            }

            private static T GetOptionOrDefault<T>(AnalyzerConfigOptions? options, Option2<T> option)
            {
                if (options is null)
                    return option.DefaultValue;

                return options.GetOption(option);
            }
        }
    }
}

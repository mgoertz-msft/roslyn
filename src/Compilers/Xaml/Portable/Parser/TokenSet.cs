// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{

    [DebuggerDisplay("{DebuggerDisplayValue,nq}")]
    internal struct TokenSet
    {
        private uint bits0;
        public static TokenSet operator |(TokenSet ts, SyntaxKind t)
        {
            TokenSet result = new TokenSet();
            int i = (int)t;
            result.bits0 = ts.bits0 | (1u << i);
            return result;
        }
        public static TokenSet operator |(TokenSet ts1, TokenSet ts2)
        {
            TokenSet result = new TokenSet();
            result.bits0 = ts1.bits0 | ts2.bits0;
            return result;
        }
        internal bool this[SyntaxKind t]
        {
            get
            {
                int i = (int)t;
                return (this.bits0 & (1ul << i)) != 0;
            }
        }
#if DEBUG
        [SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static TokenSet()
        {
            int i = (int)SyntaxKind.EndOfFile;
            Debug.Assert(0 <= i && i <= 31, "Change bits0 to ulong"); // CPJ REVIEWED
        }

        private string DebuggerDisplayValue
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                bool first = true;
                for (SyntaxKind t = SyntaxKind.None; t <= SyntaxKind.EndOfFile; t++)
                {
                    if ((bits0 & (1u << (int)t)) != 0)
                    {
                        if (!first)
                        {
                            builder.Append(", ");
                        }

                        first = false;
                        builder.Append(t);
                    }
                }

                if (builder.Length == 0)
                {
                    builder.Append("<empty>");
                }

                return builder.ToString();
            }
        }
#endif
    }
}
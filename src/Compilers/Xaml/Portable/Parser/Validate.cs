// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    using DiagDebug = System.Diagnostics.Debug;

    internal static class Validate
    {
        public static void Retail(bool value)
        {
            if (!value) Fail();
        }

        [Conditional("DEBUG")]
        public static void Debug(bool value)
        {
            if (!value) Fail();
        }

        [Conditional("DEBUG")]
        public static void Debug(bool value, string message)
        {
            if (!value) Fail(message);
        }

        [Conditional("DEBUG")]
        public static void Debug(bool value, string message, params object[] args)
        {
            if (!value) Fail(message, args);
        }

        [Conditional("DEBUG")]
        public static void DebugFail()
        {
            Fail();
        }

        [Conditional("DEBUG")]
        public static void DebugFail(string message)
        {
            Fail(message);
        }

        [Conditional("DEBUG")]
        public static void DebugFail(string message, params object[] args)
        {
            Fail(message, args);
        }

        private static void Fail()
        {
        }

        private static void Fail(string message)
        {
        }

        private static void Fail(string message, object[] args)
        {
        }
    }
}
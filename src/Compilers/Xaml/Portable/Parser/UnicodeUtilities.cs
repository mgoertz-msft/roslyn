// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Xaml.Syntax.InternalSyntax
{

    internal static class UnicodeUtilities
    {

        public static bool IsValidUnicodeValue(int value)
        {
            // Test value for valid unicode values as specified in http://www.w3.org/TR/REC-xml/#charsets.
            // More common unicode values earlier in condition for better perf.
            return (value == 0xD) ||
                ((value >= 0x20) && (value <= 0xD7FF)) ||
                ((value >= 0xE000) && (value <= 0xFFFD)) ||
                (value == 0x9) || (value == 0xA) ||
                ((value >= 0x10000) && (value <= 0x10FFFF));
        }

        /// <summary>
        /// When a Unicode value is >FFFF (Unicode plane is > 0), it needs to be split into a surrogate pair.
        /// The process is:
        /// - subtract 0x10000, leaving a value between 0x0 and 0xFFFFF (20 bits). 
        /// - The high 10 bits are added to 0xD800, giving the lead surrogate value
        /// - the low 10 bits are added to 0xDC00, giving the trail surrogate value
        /// - these two values are converted to chars, and concatenated high-low, resulting in a 2-char string,
        ///   which represents the higher plane Unicode code point.
        /// </summary>
        public static string IntToUtf16String(int value)
        {
            if (value > 0xFFFF)
            {
                // A Unicode surrogate pair needs to be created, in order to return UTF16
                return new string(new char[] { 
                    System.Convert.ToChar(((value - 0x10000) >> 10) + 0xD800), // lead surrogate
                    System.Convert.ToChar(((value - 0x10000) & 0x3FF) + 0xDC00) }); // trail surrogate
            }
            else
            {
                return System.Convert.ToChar(value).ToString();
            }
        }
    }
}

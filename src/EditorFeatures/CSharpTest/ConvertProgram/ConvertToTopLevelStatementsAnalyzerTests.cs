﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeStyle;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.CodeStyle;
using Microsoft.CodeAnalysis.CSharp.ConvertProgram;
using Microsoft.CodeAnalysis.CSharp.TopLevelStatements;
using Microsoft.CodeAnalysis.Editor.UnitTests.CodeActions;
using Microsoft.CodeAnalysis.Testing;
using Xunit;

namespace Microsoft.CodeAnalysis.Editor.CSharp.UnitTests.ConvertProgram
{
    using VerifyCS = CSharpCodeFixVerifier<ConvertToTopLevelStatementsDiagnosticAnalyzer, ConvertToTopLevelStatementsCodeFixProvider>;

    public class ConvertToTopLevelStatementsAnalyzerTests
    {
        [Fact]
        public async Task NotOfferedWhenUserPrefersProgramMain()
        {
            var code = @"
class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, false } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOfferedPriorToCSharp9()
        {
            var code = @"
class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp8,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true } },
            }.RunAsync();
        }

        [Fact]
        public async Task OfferedInCSharp9()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    {|IDE0210:static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }|}
}
",
                FixedCode = @"System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true } },
            }.RunAsync();
        }

        [Fact]
        public async Task OfferedOnNameWhenNotHidden()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
",
                FixedCode = @"System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnNonStaticMain()
        {
            var code = @"
class Program
{
    void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
                ExpectedDiagnostics =
                {
                    // error CS5001: Program does not contain a static 'Main' method suitable for an entry point
                    DiagnosticResult.CompilerError("CS5001"),
                }
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnGenericMain()
        {
            var code = @"
class Program
{
    static void Main<T>(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
                ExpectedDiagnostics =
                {
                    // error CS5001: Program does not contain a static 'Main' method suitable for an entry point
                    DiagnosticResult.CompilerError("CS5001"),
                }
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnRandomMethod()
        {
            var code = @"
class Program
{
    static void Main1(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
                ExpectedDiagnostics =
                {
                    // error CS5001: Program does not contain a static 'Main' method suitable for an entry point
                    DiagnosticResult.CompilerError("CS5001"),
                }
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnMethodWithNoBody()
        {
            var code = @"
class Program
{
    static void {|CS0501:Main|}(string[] args);
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnExpressionBody()
        {
            // we could choose to support this in the future.  It's not supported for now for simplicity.
            var code = @"
class Program
{
    static void Main(string[] args)
        => System.Console.WriteLine(0);
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnTypeWithInheritance1()
        {
            var code = @"
class Program : System.Exception
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnTypeWithInheritance2()
        {
            var code = @"
class Program : {|CS0535:System.IComparable|}
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnMultiPartType()
        {
            var code = @"
partial class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}

partial class Program
{
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnPublicType()
        {
            var code = @"
public class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnTypeWithAttribute()
        {
            var code = @"
[System.CLSCompliant(true)]
class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnTypeWithDocComment()
        {
            var code = @"
/// <summary></summary>
class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotOnTypeWithNormalComment()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
// <summary></summary>
class Program
{
    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
",
                FixedCode = @"System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithMemberWithAttributes()
        {
            var code = @"
class Program
{
    [System.CLSCompliant(true)]
    static int x;

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithMemberWithDocComment()
        {
            var code = @"
class Program
{
    /// <summary></summary>
    static int x;

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithNonPrivateMember()
        {
            var code = @"
class Program
{
    public static int x;

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithNonStaticMember()
        {
            var code = @"
class Program
{
    int x;

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithStaticConstructor()
        {
            var code = @"
class Program
{
    static Program()
    {
    }

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithInstanceConstructor()
        {
            var code = @"
class Program
{
    private Program()
    {
    }

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithProperty()
        {
            var code = @"
class Program
{
    private int X { get; }

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithEvent()
        {
            var code = @"
class Program
{
    private event System.Action X;

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithOperator()
        {
            var code = @"
class Program
{
    public static Program operator+(Program p1, Program p2) => null;

    static void Main(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task NotWithMethodWithWrongArgsName()
        {
            var code = @"
class Program
{
    static void Main(string[] args1)
    {
        System.Console.WriteLine(0);
    }
}
";

            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestFieldWithNoAccessibility()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    static int x;

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
",
                FixedCode = @"int x;

System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestFieldWithPrivateAccessibility()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    private static int x;

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
",
                FixedCode = @"int x;

System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestFieldWithMultipleDeclarators()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    private static int x, y;

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
",
                FixedCode = @"int x, y;

System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestFieldWithInitializer()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    private static int x = 0;

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
",
                FixedCode = @"int x = 0;

System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestFieldWithComments()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    // Leading
    private static int x = 0; // Trailing

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(0);
    }
}
",
                FixedCode = @"// Leading
int x = 0; // Trailing

System.Console.WriteLine(0);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestEmptyMethod()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    private static int x = 0;

    static void {|IDE0210:Main|}(string[] args)
    {
    }
}
",
                FixedCode = @"int x = 0;
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestMultipleStatements()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    private static int x = 0;

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(args);
        return;
    }
}
",
                FixedCode = @"int x = 0;

System.Console.WriteLine(args);
return;
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestOtherMethodBecomesLocalFunction()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
class Program
{
    private static int x = 0;

    static void OtherMethod()
    {
        return;
    }

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(args);
    }
}
",
                FixedCode = @"int x = 0;

void OtherMethod()
{
    return;
}

System.Console.WriteLine(args);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestNotWithUnsafeMethod()
        {
            var code = @"
class Program
{
    private static int x = 0;

    unsafe static void OtherMethod()
    {
        return;
    }

    static void Main(string[] args)
    {
        System.Console.WriteLine(args);
    }
}
";
            await new VerifyCS.Test
            {
                TestCode = code,
                FixedCode = code,
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }

        [Fact]
        public async Task TestOtherComplexMethodBecomesLocalFunction()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
using System.Threading.Tasks;

class Program
{
    private static int x = 0;

    static async Task OtherMethod<T>(T param) where T : struct
    {
        return;
    }

    static void {|IDE0210:Main|}(string[] args)
    {
        System.Console.WriteLine(args);
    }
}
",
                FixedCode = @"
using System.Threading.Tasks;

int x = 0;

async Task OtherMethod<T>(T param) where T : struct
{
    return;
}

System.Console.WriteLine(args);
",
                LanguageVersion = LanguageVersion.CSharp9,
                TestState = { OutputKind = OutputKind.ConsoleApplication },
                Options = { { CSharpCodeStyleOptions.PreferTopLevelStatements, true, NotificationOption2.Suggestion } },
            }.RunAsync();
        }
    }
}

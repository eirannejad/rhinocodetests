using System;
using System.Linq;
using System.Collections.Generic;

using NUnit.Framework;

using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Execution;
using Rhino.Runtime.Code.Execution.Debugging;
using Rhino.Runtime.Code.Execution.Profiling;
using Rhino.Runtime.Code.Languages;
using Rhino.Runtime.Code.Testing;

namespace RhinoCodePlatform.Rhino3D.Tests
{
    [TestFixture]
    public class CSharp_Tests : ScriptFixture
    {
        [Test, TestCaseSource(nameof(GetTestScripts))]
        public void TestCSharp_Script(ScriptInfo scriptInfo)
        {
            TestSkip(scriptInfo);

            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(scriptInfo.Uri);

            RunContext ctx;
            if (scriptInfo.IsDebug)
                ctx = new DebugContext();
            else
                ctx = new RunContext();

            ctx.AutoApplyParams = true;
            ctx.OutputStream = GetOutputStream();
            ctx.Outputs["result"] = default;

            if (TryRunCode(scriptInfo, code, ctx, out string errorMessage))
            {
                Assert.True(ctx.Outputs.TryGet("result", out bool data));
                Assert.True(data);
            }
            else
                Assert.True(scriptInfo.MatchesError(errorMessage));
        }

        [Test]
        public void TestCSharp_CompileErrorLine_MissingFunction()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;

DoOtherStuff();

void DoStuff(int s)
{
    // try
    // {
    var k;



    var z = Joe.One;
}
");

            var ctx = new BuildContext();

            try
            {
                code.Build(ctx);
            }
            catch (CompileException ex)
            {
                if (ex.Diagnostics.First().Reference.Position.LineNumber != 4)
                    throw;
            }
        }

        [Test]
        public void TestCSharp_Compile_Script()
        {
            // assert throws compile exception on run/debug/profile
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
using Rhino;

a = x + y;
b = new Sphere(Point3d.Origin, x);
");


            code.DebugControls = new DebugContinueAllControls();

            ExecuteException run = Assert.Throws<ExecuteException>(() => code.Run(new RunContext()));
            Assert.IsInstanceOf(typeof(CompileException), run.InnerException);

            ExecuteException debug = Assert.Throws<ExecuteException>(() => code.Debug(new DebugContext()));
            Assert.IsInstanceOf(typeof(CompileException), debug.InnerException);

            ExecuteException profile = Assert.Throws<ExecuteException>(() => code.Profile(new ProfileContext()));
            Assert.IsInstanceOf(typeof(CompileException), profile.InnerException);
        }

        [Test]
        public void TestCSharp_RuntimeErrorLine_InScript()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;

int a = 1 + 2;
int zero = 0;
a = 5 / zero;
");

            RunContext ctx = GetRunContext();

            try
            {
                code.Run(ctx);
            }
            catch (ExecuteException ex)
            {
                if (ex.Position.LineNumber != 6)
                    throw;
            }
        }

        [Test]
        public void TestCSharp_RuntimeErrorLine_InFunction()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;

DoOtherStuff();

void DoStuff(int s)
{
    // try
    // {
    var k = 12;



    var z = Joe.One;
    var y = new Jose();
    var m = new Uri(""file:///"");
    var n = new Jack();
    
    
    var f = 12 / s;

    // Console.WriteLine(12 / s);
    // }
    // catch (Exception ex)
    // {
    //     Console.WriteLine(ex);
    // }
}

void DoOtherStuff() { DoStuff(0); }

enum Joe {
    One,
    Two,
}

class Jack { }

struct Jose { }

");

            RunContext ctx = GetRunContext();

            try
            {
                code.Run(ctx);
            }
            catch (ExecuteException ex)
            {
                if (ex.Position.LineNumber != 20)
                    throw;
            }
        }

        [Test]
        public void TestCSharp_DebugStop()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
Console.WriteLine(); // line 3
");

            // create debug controls to capture exception on line 9
            // note that a breakpoint on the line must be added. the controls
            // will stepOver the line to capture the exception event.
            var breakpoint = new CodeReferenceBreakpoint(code, 3);
            var controls = new DebugStopperControls(breakpoint);

            var ctx = new DebugContext();

            code.DebugControls = controls;


            Assert.Throws<DebugStopException>(() => code.Debug(ctx));
        }

        [Test]
        public void TestCSharp_DebugPauses_Script()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
using Rhino;
using Rhino.Geometry;

Console.WriteLine(""RunScript"");
a = x + y; // line 7

var sphere = new Sphere(Point3d.Origin, x);
b = (int)sphere.Radius;
");

            var breakpoint = new CodeReferenceBreakpoint(code, 7);
            var controls = new DebugPauseDetectControls(breakpoint);

            using var swallow = new SwallowOutputsStream();
            code.DebugControls = controls;
            code.Debug(new DebugContext
            {
                AutoApplyParams = true,
                OutputStream = swallow,
                Inputs =
                {
                    ["x"] = 21,
                    ["y"] = 21,
                },
                Outputs =
                {
                    ["a"] = 0,
                    ["b"] = 0,
                }
            });

            Assert.True(controls.Pass);
        }

        [Test]
        public void TestCSharp_DebugPauses_ScriptInstance()
        {
            const string INSTANCE = "__instance__";

            // detect missing variables in global scope does not break debugging.
            // this could happen if roslyn trace-injector does not produce valid code
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
$@"
using System;
using Rhino;

public class Script_Instance
{{
  public void RunScript(double x, double y, ref object a)
  {{
    a = x + y; // line 9
  }}
}}

{INSTANCE} = new Script_Instance();
");

            using DebugContext instctx = new() { AutoApplyParams = true, Outputs = { [INSTANCE] = default } };
            code.Run(instctx);
            dynamic instance = instctx.Outputs.Get(INSTANCE);

            var breakpoint = new CodeReferenceBreakpoint(code, 9);
            var controls = new DebugPauseDetectControls(breakpoint);
            code.DebugControls = controls;

            using (DebugContext ctx = new())
            {
                using DebugGroup g = code.DebugWith(ctx, invokes: true);
                object a = default;
                instance.RunScript(21, 21, ref a);
            }

            Assert.True(controls.Pass);
        }

        [Test]
        public void TestCSharp_Debug_Script()
        {
            // detect auto-declare code params are in global scope.
            // this could happen if roslyn trace-injector does not produce valid code
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
using Rhino;

a = x + y; // line 5
");

            var breakpoint = new CodeReferenceBreakpoint(code, 5);
            var controls = new DebugVerifyVarsControls(breakpoint, new ExecVariable[]
            {
                new("x", 21),
                new("y", 21),
            });

            code.DebugControls = controls;
            var ctx = new DebugContext
            {
                AutoApplyParams = true,
                Inputs =
                {
                    ["x"] = 21,
                    ["y"] = 21,
                },
                Outputs =
                {
                    ["a"] = 0,
                }
            };
            code.Debug(ctx);

            Assert.True(controls.Pass);
            Assert.AreEqual(ctx.Outputs.Get<int>("a"), 42);
        }

        [Test]
        public void TestCSharp_DebugBuild_ScriptInstance()
        {
            // detect missing variables in global scope does not break debugging.
            // this could happen if roslyn trace-injector does not produce valid code
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
using Rhino;

Console.WriteLine(""Test""); // line 5

public class Script_Instance
{
  public void RunScript(double x, double y, ref object a)
  {
    a = x + y;
  }
}
");

            code.Inputs.Add(new Param("x") { AutoDeclare = false });
            code.Inputs.Add(new Param("y") { AutoDeclare = false });

            var breakpoint = new CodeReferenceBreakpoint(code, 5);
            var controls = new DebugVerifyEmptyVarsControls(breakpoint);

            using var swallow = new SwallowOutputsStream();
            code.DebugControls = controls;
            code.Debug(new DebugContext() { OutputStream = swallow });

            Assert.True(controls.Pass);
        }

        // FIXME: Move csharp autocompletion to language module
        //        [Test]
        //        public void TestComplete_System_Console()
        //        {
        //            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
        //@"
        //using System;
        //using Rhino;

        //Console.");

        //            string text = code.Text;
        //            IEnumerable<CompletionInfo> completions =
        //                code.Language.Support.Complete(SupportRequest.Empty, code, text.Length);

        //            CompletionInfo cinfo;
        //            bool result = true;

        //            cinfo = completions.First(c => c.Text == "WriteLine");
        //            result &= CompletionKind.Function == cinfo.Kind;

        //            cinfo = completions.First(c => c.Text == "WindowWidth");
        //            result &= CompletionKind.Property == cinfo.Kind;

        //            Assert.True(result);
        //        }

        static IEnumerable<object[]> GetTestScripts() => GetTestScripts(@"cs\", "test_*.cs");
    }
}

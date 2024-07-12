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

#if RC8_10
using RhinoCodePlatform.Rhino3D.Languages.GH1;
#else
using RhinoCodePlatform.Rhino3D.Languages;
#endif

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
        public void TestCSharp_Runtime_NULL_Input()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
a = x is null;
");

            RunContext ctx = GetRunContext();
            ctx.Inputs.Set("x", null);
            ctx.Outputs.Set("a", false);

            code.Run(ctx);

            bool isnull = ctx.Outputs.Get<bool>("a");
            Assert.IsTrue(isnull);
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
        public void TestCSharp_DebugVars_Script()
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
            var controls = new DebugVerifyVarsControls(breakpoint, new ExpectedVariable[]
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
            Assert.AreEqual(42, ctx.Outputs.Get<int>("a"));
        }

        [Test]
        public void TestCSharp_DebugVars_ScriptInstance()
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

#if RC8_9
        [Test]
        public void TestCSharp_DebugPauses_Script_StepOut()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
void Pass() {}
void First()
{
    Pass(); // line 6
    Pass(); // line 7
}

First();
");

            var controls = new DebugPauseDetectControls();
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 6), DebugAction.StepOver);
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 7));

            code.DebugControls = controls;
            code.Debug(new DebugContext());

            Assert.True(controls.Pass);

            controls.ExpectPause(new CodeReferenceBreakpoint(code, 6), DebugAction.StepOver);

            code.Debug(new DebugContext());

            Assert.True(controls.Pass);
        }

        [Test]
        public void TestCSharp_DebugPauses_Script_DoNotStepIn()
        {
            // detect auto-declare code params are in global scope.
            // this could happen if roslyn trace-injector does not produce valid code
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
void Pass() {}
void First()
{
    Pass(); // line 6
    Pass();
}

First();
");

            var controls = new DebugPauseDetectControls();
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 6), DebugAction.StepIn);
            controls.DoNotExpectPause(new CodeReferenceBreakpoint(code, 3));

            code.DebugControls = controls;
            code.Debug(new DebugContext());

            Assert.True(controls.Pass);
        }

        [Test]
        public void TestCSharp_DebugReturn_Script()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
void Pass() {}
int Second()
{
    Pass(); // line 6
    return 12; // line 7
}
void First()
{
    Second();
}

First();
");

            var controls = new DebugPauseDetectControls();
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 6), DebugAction.StepOver);
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 7));

            code.DebugControls = controls;
            code.Debug(new DebugContext());

            Assert.True(controls.Pass);
        }

        [Test]
        public void TestCSharp_DebugNested_Script()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
void Pass() {}
void Second() {}
void First()
{
    Pass(); // line 7
    Second(); // line 8
    Pass();
}

First();
");

            var controls = new DebugPauseDetectControls();
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 7), DebugAction.StepOver);
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 8), DebugAction.Continue);

            code.DebugControls = controls;
            code.Debug(new DebugContext());

            Assert.True(controls.Pass);
        }

        [Test]
        public void TestCSharp_DebugNestedNested_Script()
        {
            Code code = GetLanguage(this, LanguageSpec.CSharp).CreateCode(
@"
using System;
void Pass() {}
int Second()
{
    Pass(); // line 6
    Pass();
    Pass();
    return 12;
}
void First()
{
    Pass();
    Second(); // line 14
    Pass();
}

First();
");

            var controls = new DebugPauseDetectControls();
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 14), DebugAction.StepIn);
            controls.ExpectPause(new CodeReferenceBreakpoint(code, 6), DebugAction.Continue);

            code.DebugControls = controls;
            code.Debug(new DebugContext());

            Assert.True(controls.Pass);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert()
        {
            var script = new Grasshopper1Script(@"
// #! csharp
// Grasshopper Script
using System;
a = ""Hello Python 3 in Grasshopper!"";
Console.WriteLine(a);
");

            script.ConvertToScriptInstance(addSolve: false, addPreview: false);

            // NOTE:
            // no params are defined so RunScript() is empty
            // FIXME:
            // comments are removed in C# conversion
            Assert.AreEqual(@"
// #! csharp
// Grasshopper Script
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

public class Script_Instance : GH_ScriptInstance
{
  /* 
    Members:
      RhinoDoc RhinoDocument
      GH_Document GrasshopperDocument
      IGH_Component Component
      int Iteration

    Methods (Virtual & overridable):
      Print(string text)
      Print(string format, params object[] args)
      Reflect(object obj)
      Reflect(object obj, string method_name)
  */

  private void RunScript()
  {
    a = ""Hello Python 3 in Grasshopper!"";
    Console.WriteLine(a);
  }
}

", script.Text);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert_LastEmptyLine()
        {
            var script = new Grasshopper1Script(@"
// #! csharp
// Grasshopper Script
using System;
a = ""Hello Python 3 in Grasshopper!"";
Console.WriteLine(a);");

            script.ConvertToScriptInstance(addSolve: false, addPreview: false);

            // NOTE:
            // no params are defined so RunScript() is empty
            Assert.AreEqual(@"
// #! csharp
// Grasshopper Script
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

public class Script_Instance : GH_ScriptInstance
{
  /* 
    Members:
      RhinoDoc RhinoDocument
      GH_Document GrasshopperDocument
      IGH_Component Component
      int Iteration

    Methods (Virtual & overridable):
      Print(string text)
      Print(string format, params object[] args)
      Reflect(object obj)
      Reflect(object obj, string method_name)
  */

  private void RunScript()
  {
    a = ""Hello Python 3 in Grasshopper!"";
    Console.WriteLine(a);  }
}

", script.Text);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert_WithFunction()
        {
            // https://mcneel.myjetbrains.com/youtrack/issue/RH-82125
            var script = new Grasshopper1Script(@"
// #! csharp
// Grasshopper Script
using System;
a = ""Hello Python 3 in Grasshopper!"";
Console.WriteLine(a);

int Test()
{
    return 42;
}
");

            script.ConvertToScriptInstance(addSolve: false, addPreview: false);

            // NOTE:
            // no params are defined so RunScript() is empty
            Assert.AreEqual(@"
// #! csharp
// Grasshopper Script
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

public class Script_Instance : GH_ScriptInstance
{
  /* 
    Members:
      RhinoDoc RhinoDocument
      GH_Document GrasshopperDocument
      IGH_Component Component
      int Iteration

    Methods (Virtual & overridable):
      Print(string text)
      Print(string format, params object[] args)
      Reflect(object obj)
      Reflect(object obj, string method_name)
  */

  private void RunScript()
  {
    a = ""Hello Python 3 in Grasshopper!"";
    Console.WriteLine(a);
  }

  private int Test()
  {
    return 42;
  }
}

", script.Text);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert_WithFunctionWithParams()
        {
            // https://mcneel.myjetbrains.com/youtrack/issue/RH-82125
            var script = new Grasshopper1Script(@"
// #! csharp
// Grasshopper Script
using System;
a = ""Hello Python 3 in Grasshopper!"";
Console.WriteLine(a);

int Test(int x, int y)
{
    return 42;
}
");

            script.ConvertToScriptInstance(addSolve: false, addPreview: false);

            // NOTE:
            // no params are defined so RunScript() is empty
            Assert.AreEqual(@"
// #! csharp
// Grasshopper Script
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using Rhino;
using Rhino.Geometry;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

public class Script_Instance : GH_ScriptInstance
{
  /* 
    Members:
      RhinoDoc RhinoDocument
      GH_Document GrasshopperDocument
      IGH_Component Component
      int Iteration

    Methods (Virtual & overridable):
      Print(string text)
      Print(string format, params object[] args)
      Reflect(object obj)
      Reflect(object obj, string method_name)
  */

  private void RunScript()
  {
    a = ""Hello Python 3 in Grasshopper!"";
    Console.WriteLine(a);
  }

  private int Test(int x, int y)
  {
    return 42;
  }
}

", script.Text);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert_AddSolveOverrides()
        {
            var script = new Grasshopper1Script(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }
}
");

            script.ConvertToScriptInstance(addSolve: true, addPreview: false);

            Assert.AreEqual(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }

    public override void BeforeRunScript()
    {
    }

    public override void AfterRunScript()
    {
    }
}
", script.Text);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert_AddPreviewOverrides()
        {
            var script = new Grasshopper1Script(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }
}
");

            script.ConvertToScriptInstance(addSolve: false, addPreview: true);

            Assert.AreEqual(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }

    public override BoundingBox ClippingBox => BoundingBox.Empty;

    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
    }

    public override void DrawViewportWires(IGH_PreviewArgs args)
    {
    }
}
", script.Text);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert_AddBothOverrides()
        {
            var script = new Grasshopper1Script(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }
}
");

            script.ConvertToScriptInstance(addSolve: true, addPreview: true);

            Assert.AreEqual(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }

    public override void BeforeRunScript()
    {
    }

    public override void AfterRunScript()
    {
    }

    public override BoundingBox ClippingBox => BoundingBox.Empty;

    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
    }

    public override void DrawViewportWires(IGH_PreviewArgs args)
    {
    }
}
", script.Text);
        }

        [Test]
        public void TestCSharp_ScriptInstance_Convert_AddBothOverrides_Steps()
        {
            var script = new Grasshopper1Script(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }
}
");

            script.ConvertToScriptInstance(addSolve: true, addPreview: false);
            script.ConvertToScriptInstance(addSolve: false, addPreview: true);

            Assert.AreEqual(@"// #! csharp
using System;
using Rhino;
using Grasshopper;

public class Script_Instance : GH_ScriptInstance
{
    private void RunScript()
    {
    }

    public override void BeforeRunScript()
    {
    }

    public override void AfterRunScript()
    {
    }

    public override BoundingBox ClippingBox => BoundingBox.Empty;

    public override void DrawViewportMeshes(IGH_PreviewArgs args)
    {
    }

    public override void DrawViewportWires(IGH_PreviewArgs args)
    {
    }
}
", script.Text);
        }
#endif

        // FIXME: Move csharp autocompletion to language module
//        [Test]
//        public void TestCSharp_ScriptInstance_Complete_Self()
//        {
//            var script = new Grasshopper1Script(@"// #! csharp
//using System;
//using Rhino;
//using Grasshopper;

//public class Script_Instance : GH_ScriptInstance
//{
//    private void RunScript()
//    {
//        this.
//    }
//}
//");

//            Code code = script.CreateCode();

//            IEnumerable<CompletionInfo> completions =
//                code.Language.Support.Complete(SupportRequest.Empty, code, 168, CompleteOptions.Empty);

//            CompletionInfo cinfo;
//            bool result = true;

//            cinfo = completions.First(c => c.Text == "Iteration");
//            result &= CompletionKind.Property == cinfo.Kind;

//            cinfo = completions.First(c => c.Text == "RhinoDocument");
//            result &= CompletionKind.Property == cinfo.Kind;

//            cinfo = completions.First(c => c.Text == "GrasshopperDocument");
//            result &= CompletionKind.Property == cinfo.Kind;

//            cinfo = completions.First(c => c.Text == "Component");
//            result &= CompletionKind.Property == cinfo.Kind;

//            cinfo = completions.First(c => c.Text == "Print");
//            result &= CompletionKind.Function == cinfo.Kind;

//            cinfo = completions.First(c => c.Text == "Reflect");
//            result &= CompletionKind.Function == cinfo.Kind;

//            cinfo = completions.First(c => c.Text == "AddRuntimeMessage");
//            result &= CompletionKind.Function == cinfo.Kind;

//            Assert.True(result);
//        }

//        [Test]
//        public void TestCSharp_ScriptInstance_Complete_SelfRhinoDoc()
//        {
//            var script = new Grasshopper1Script(@"// #! csharp
//using System;
//using Rhino;
//using Grasshopper;

//public class Script_Instance : GH_ScriptInstance
//{
//    private void RunScript()
//    {
//        this.RhinoDocument.
//    }
//}
//");

//            Code code = script.CreateCode();

//            IEnumerable<CompletionInfo> completions =
//                code.Language.Support.Complete(SupportRequest.Empty, code, 182, CompleteOptions.Empty);

//            CompletionInfo cinfo;
//            bool result = true;

//            cinfo = completions.First(c => c.Text == "ActiveCommandId");
//            result &= CompletionKind.Property == cinfo.Kind;

//            cinfo = completions.First(c => c.Text == "OpenDocuments");
//            result &= CompletionKind.Function == cinfo.Kind;

//            Assert.True(result);
//        }

        static IEnumerable<object[]> GetTestScripts() => GetTestScripts(@"cs\", "test_*.cs");
    }
}

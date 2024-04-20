// async: true
// r "Grasshopper"
// r "GH_IO"
using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

using Rhino;

using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Languages;
using Rhino.Runtime.Code.Execution;

using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

var gh = RhinoCode.Languages.QueryLatest(new LanguageSpec("*.*.grasshopper", "1"));
var code = gh.CreateCode(@"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");

var input = new GH_Structure<GH_Integer>();
input.Append(new GH_Integer(21));

var output = new GH_Structure<GH_Integer>();

var ctx = new RunContext
{
  Inputs = {
        ["A"] = input,
        ["B"] = input,
    },
  Outputs = {
        ["C"] = output,
    },
  Options = {
        ["grasshopper.runAsCommand"] = false
    }
};

var ts = new List<Task>();

ts.Add(Task.Run(() => code.Run(ctx)));

Task.WaitAll(ts.ToArray());

if (ctx.Outputs.TryGet("C", out IGH_Structure data))
{
  foreach (var p in data.Paths)
  {
    foreach (var d in data.get_Branch(p))
      RhinoApp.WriteLine(d.ToString());
  }
}

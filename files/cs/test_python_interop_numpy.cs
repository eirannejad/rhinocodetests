// #! csharp
/*
   C# script in Rhino is using rhinocode api to run a python 3 script (in the same process)
   and grab the results.
*/
using System;
using System.Linq;
using System.Collections.Generic;

using Rhino;

using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Languages;
using Rhino.Runtime.Code.Execution;

// UNPUBLISHED API -- DO NOT SHARE
// find python language
var py3Lang = RhinoCode.Languages.QueryLatest(LanguageSpec.Python3);

// create a python code
// note the `# r: numpy` part in the code
// this makes runtime install numpy if does not exist
var py3Code = py3Lang.CreateCode(@"
# r: numpy

import numpy
randoms = list(numpy.random.rand(10))
");

// create a run context
// this hold inputs and output values
var ctx = new RunContext
{
    AutoApplyParams = true,
    
    // initialize output value
    Outputs = {
        ["randoms"] = null,
    }
};

bool test = true;

try
{
    // run the code
    py3Code.Run(ctx);

    // grab the result
    // python is not typed to lets cast it to a generic List of objects
    if (ctx.Outputs.TryGet("randoms", out List<object> data))
    {
        test = true;
    }
    else
        test = false;
}
catch (Exception ex)
{
    test = false;
}


// Console.WriteLine(test);
result = test;
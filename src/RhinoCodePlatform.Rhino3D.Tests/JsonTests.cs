using System;
using System.Collections.Generic;

using NUnit.Framework;

using Rhino.Runtime.Code;
using Rhino.Runtime.Code.Execution;
using Rhino.Runtime.Code.Languages;

namespace RhinoCodePlatform.Rhino3D.Tests
{
    [TestFixture]
    public class JsonTests : ScriptFixture
    {
        [Test, TestCaseSource(nameof(GetTestScripts))]
        public void TestJsonScript(ScriptInfo scriptInfo)
        {
            TestSkip(scriptInfo);

            // no exec for json. just make sure code can be created
            Code _ = GetLanguage(this, LanguageSpec.JSON).CreateCode(scriptInfo.Uri);
        }

        static IEnumerable<object[]> GetTestScripts() => GetTestScripts(@"json\", "test_*.json");
    }
}

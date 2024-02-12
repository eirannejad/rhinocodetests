using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rhino.Testing
{
    // https://docs.nunit.org/articles/vs-test-adapter/AdapterV4-Release-Notes.html
    // https://github.com/nunit/nunit3-vs-adapter/blob/master/src/NUnitTestAdapter/AdapterSettings.cs#L143
    static class RhinoCore
    {
        static string s_systemDirectory;
        static IDisposable s_core;
        static bool s_inRhino = false;

        public static void Initialize()
        {
            if (s_core is null)
            {
                s_systemDirectory = Configs.Current.RhinoSystemDir;

                AppDomain.CurrentDomain.AssemblyResolve += ResolveForRhinoAssemblies;

                TestContext.WriteLine("Loading rhino core");
                LoadCore();

                TestContext.WriteLine("Loading eto platform");
                LoadEto();
            }
        }

        public static void TearDown()
        {
            if (s_core is IDisposable)
            {
                DisposeCore();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadCore()
        {
            s_inRhino = Process.GetCurrentProcess().ProcessName.Equals("Rhino");
            if (s_inRhino)
            {
                return;
            }

#if NET7_0_OR_GREATER
            string[] args = new string[] { "/netcore " };
#else
            string[] args = new string[] { "/netfx " };
#endif

            s_core = new Rhino.Runtime.InProcess.RhinoCore(args);

            // ensure RhinoCommon and its associated native libraries are ready
            Rhino.Runtime.HostUtils.InitializeRhinoCommon();

            // ensure RDK and its associated native libraries are ready
            // rdk.rhp plugin must be loaded before the rdk native library
            string rdkRhp = Path.Combine(s_systemDirectory, "Plug-ins", "rdk.rhp");
            Rhino.PlugIns.PlugIn.LoadPlugIn(rdkRhp, out Guid _);

            Rhino.Runtime.HostUtils.InitializeRhinoCommon_RDK();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void LoadEto()
        {
            Eto.Platform.AllowReinitialize = true;
            Eto.Platform.Initialize(Eto.Platforms.Wpf);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void DisposeCore()
        {
            s_inRhino = false;

            ((Rhino.Runtime.InProcess.RhinoCore)s_core).Dispose();
            s_core = null;
        }

        static Assembly ResolveForRhinoAssemblies(object sender, ResolveEventArgs args)
        {
            bool netcore =
                System.Environment.Version.Major >= 5
             || RuntimeInformation.FrameworkDescription.StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase);

            string name = new AssemblyName(args.Name).Name;

            foreach (string path in new List<string>
            {
                Path.Combine(s_systemDirectory, netcore ? "netcore" : "netfx"),
                s_systemDirectory,
                typeof(RhinoCore).Assembly.Location,
                Path.Combine(s_systemDirectory, @"Plug-ins\Grasshopper"),
            })
            {
                string file = Path.Combine(path, name + ".dll");
                if (File.Exists(file))
                {
                    TestContext.WriteLine($"Loading assembly from file {file}");
                    return Assembly.LoadFrom(file);
                }
            }

            TestContext.WriteLine($"Could not find assembly {name}");
            return null;
        }
    }
}



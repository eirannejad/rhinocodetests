using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

using NUnit.Framework;

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

                TestContext.WriteLine("Loading grasshopper (headless)");
                LoadGrasshopper();
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

            s_core = new Rhino.Runtime.InProcess.RhinoCore();

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
        static void LoadGrasshopper()
        {
            string ghPlugin = Path.Combine(s_systemDirectory, @"Plug-ins\Grasshopper", "GrasshopperPlugin.rhp");
            Rhino.PlugIns.PlugIn.LoadPlugIn(ghPlugin, out Guid _);

            object ghObj = Rhino.RhinoApp.GetPlugInObject("Grasshopper");
            if (ghObj?.GetType().GetMethod("RunHeadless") is MethodInfo runHeadLess)
                runHeadLess.Invoke(ghObj, null);
            else
                TestContext.WriteLine("Failed loading grasshopper (Headless)");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static void DisposeCore()
        {
            s_inRhino = false;

            ((Rhino.Runtime.InProcess.RhinoCore)s_core).Dispose();
            s_core = null;
        }

        static readonly string[] s_subpaths = new string[]
        {
            @"Plug-ins\Grasshopper",
        };

        static Assembly ResolveForRhinoAssemblies(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;

            string file = Path.Combine(s_systemDirectory, name + ".dll");
            if (File.Exists(file))
            {
                TestContext.WriteLine($"Loading assembly from file {file}");
                return Assembly.LoadFrom(file);
            }

            foreach (var plugin in s_subpaths)
            {
                file = Path.Combine(s_systemDirectory, plugin, name + ".dll");
                if (File.Exists(file))
                {
                    TestContext.WriteLine($"Loading plugin assembly from file {file}");
                    return Assembly.LoadFrom(file);
                }
            }

            TestContext.WriteLine($"Could not find assembly {name}");
            return null;
        }
    }
}



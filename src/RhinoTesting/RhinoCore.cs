﻿using System;
using System.IO;
using System.Reflection;

namespace Rhino.Testing
{
    static class RhinoCore
    {
        static string _systemDirectory;
        static IDisposable _core;

        public static void Initialize(RhinoTestConfigs configs)
        {
            if (_core is null)
            {
                _systemDirectory = configs.RhinoSystemDir;

                RhinoInside.Resolver.Initialize();
                RhinoInside.Resolver.RhinoSystemDirectory = _systemDirectory;
                AppDomain.CurrentDomain.AssemblyResolve += ResolveForRhinoAssemblies;
                LoadCore();
            }
        }

        public static void TearDown()
        {
            _core?.Dispose();
            _core = null;
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        static void LoadCore()
        {
            _core = new Rhino.Runtime.InProcess.RhinoCore();
        }

        static readonly string[] s_pluginpaths = new string[]
        {
            @"Plug-ins\Grasshopper",
        };

        static Assembly ResolveForRhinoAssemblies(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name;

            foreach (var plugin in s_pluginpaths)
            {
                string file = Path.Combine(_systemDirectory, plugin, name + ".dll");
                if (File.Exists(file))
                    return Assembly.LoadFrom(file);
            }

            return null;
        }
    }
}



using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PowerTools.Helpers
{
    public static class AssembliesResolver
    {
        private static bool IsRegistered = false;
        private static Dictionary<string, Assembly> DependentAssemblies = new Dictionary<string, Assembly>();

        public static Assembly Resolve(Assembly? requestingAssembly, string assemblyFullQualifiedName)
        {
            AssemblyName assemblyName = new AssemblyName(assemblyFullQualifiedName);
            string assemblyShortName = assemblyName.Name;
            var hasLoadedAssemblies = DependentAssemblies.ContainsKey(assemblyShortName);

            if (!hasLoadedAssemblies)
            {
                var assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
                if (requestingAssembly != null)
                {
                    assemblyPath = Path.GetDirectoryName(requestingAssembly.Location);
                }

                string fullPath = Path.Combine(assemblyPath, $"{assemblyShortName}.dll");

                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(fullPath);
                    DependentAssemblies[assemblyShortName] = assembly;
                }
                catch
                {
                    assembly = null;
                }

                return assembly;
            }

            return null;
        }

        public static void RegisterDependencyResolver()
        {
            if (!IsRegistered)
            {
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => Resolve(args.RequestingAssembly, args.Name);
                IsRegistered = true;
            }
        }
    }
}

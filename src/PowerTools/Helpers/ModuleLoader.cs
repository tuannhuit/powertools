using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PowerTools.Helpers
{
    public static class ModuleLoader
    {
        public static void LoadModule(IContainerProvider container, ToolModule module)
        {
            if (module == null) 
                throw new Exception("Cannot handle empty module!");

            var moduleLocation = module.ExecutionLocation;

            if (string.IsNullOrEmpty(moduleLocation) || string.IsNullOrWhiteSpace(moduleLocation))
                throw new Exception("Could not identify the module execution path!");

            if (!File.Exists(moduleLocation))
                throw new FileNotFoundException($"Could not find the execution module path! {moduleLocation}");

            var moduleAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(p => p.FullName == typeof(IModule).Assembly.FullName);
            var IModuleType = moduleAssembly.GetType(typeof(IModule).FullName);

            var assembly = Assembly.LoadFile(moduleLocation);

            var moduleInfos = assembly.GetExportedTypes()
                .Where(IModuleType.IsAssignableFrom)
                .Where(t => t != IModuleType)
                .Where(t => !t.IsAbstract)
                .Select(t => CreateModuleInfo(t));

            var moduleManager = container.Resolve<IModuleManager>();
            var moduleCatalog = container.Resolve<IModuleCatalog>();

            foreach (var moduleInfo in moduleInfos)
            {
                if (!moduleCatalog.Modules.ToList().Exists(p => p.ModuleName == moduleInfo.ModuleName))
                {
                    moduleCatalog.AddModule(moduleInfo);
                    ApplicationService.Instance.InvokeUIAction(() =>
                    {
                        moduleManager.LoadModule(moduleInfo.ModuleName);
                    });
                }
            }

            LoggingService.Instance.Info($"Loaded module {module.ExecutionLocation}");
        }

        private static ModuleInfo CreateModuleInfo(Type type)
        {
            if (type == null || type.Assembly == null)
            {
                throw new Exception("Failed to create a module from unknown type");
            }

            var moduleName = type.Name;
            var moduleAttribute = CustomAttributeData.GetCustomAttributes(type)
                .FirstOrDefault(p => p.Constructor.DeclaringType.FullName == typeof(ModuleAttribute).FullName);

            if (moduleAttribute != null)
            {
                foreach (var argument in moduleAttribute.NamedArguments)
                {
                    var argumentName = argument.MemberInfo.Name;
                    if (argumentName == "ModuleName")
                    {
                        moduleName = (string)argument.TypedValue.Value;
                        break;
                    }
                }
            }

            return new ModuleInfo(moduleName, type.AssemblyQualifiedName)
            {
                InitializationMode = InitializationMode.OnDemand,
                Ref = type.Assembly.CodeBase
            };
        }
    }
}

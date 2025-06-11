using PowerTools.Core.Models;
using Prism.Ioc;
using Prism.Modularity;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace PowerTools.Helpers
{
    public class ModuleLoader
    {
        private static ModuleLoader _instance;

        public static ModuleLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ModuleLoader();
                }

                return _instance;
            }
        }

        private ModuleLoader()
        {

        }

        public void LoadModule(IContainerProvider container, ToolModule module)
        {
            if (module == null)
                return;

            var modulePath = module.LocalModulePath;

            if (string.IsNullOrEmpty(modulePath))
                throw new Exception("Could not identify the module execution path!");

            if (!File.Exists(modulePath))
                throw new FileNotFoundException($"Could not find the execution module path! {modulePath}");

            var moduleAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(p => p.FullName == typeof(IModule).Assembly.FullName);
            var IModuleType = moduleAssembly.GetType(typeof(IModule).FullName);

            var assembly = Assembly.LoadFile(modulePath);

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

                    var d = Application.Current.Dispatcher;
                    if (d.CheckAccess())
                    {
                        moduleManager.LoadModule(moduleInfo.ModuleName);
                    }
                    else
                    {
                        d.BeginInvoke((Action)delegate { moduleManager.LoadModule(moduleInfo.ModuleName); });
                    }
                }
            }
        }

        private ModuleInfo CreateModuleInfo(Type type)
        {
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

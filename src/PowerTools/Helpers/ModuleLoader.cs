using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PowerTools.Helpers
{
    public static class ModuleLoader
    {
        private static IContainerProvider _container;

        public static IContainerProvider Container
        {
            get=> _container;
            set
            {
                _container = value;
                RegionManager = _container.Resolve<IRegionManager>();
                DialogRegionManager = RegionManager.CreateRegionManager();
            }
        }

        public static IRegionManager RegionManager { get; private set; }
        public static IRegionManager DialogRegionManager { get; private set; }

        public static void LoadModule(ToolModule module)
        {
            if (module.IsLoaded)
            {
                return;
            }

            if (module == null)
                throw new Exception("Cannot handle empty module!");

            var moduleLocation = module.ExecutionLocation;

            if (string.IsNullOrEmpty(moduleLocation) || string.IsNullOrWhiteSpace(moduleLocation))
                throw new Exception("Could not identify the module execution path!");

            if (!File.Exists(moduleLocation))
                throw new FileNotFoundException($"Could not find the execution module path! {moduleLocation}");


            if (Container == null)
            {
                throw new FileNotFoundException($"ModuleLoader.Container property must be setup!");
            }

            var moduleAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .First(p => p.FullName == typeof(IModule).Assembly.FullName);
            var IModuleType = moduleAssembly.GetType(typeof(IModule).FullName);

            var assembly = Assembly.LoadFile(moduleLocation);

            var moduleInfos = assembly.GetExportedTypes()
                .Where(IModuleType.IsAssignableFrom)
                .Where(t => t != IModuleType)
                .Where(t => !t.IsAbstract)
                .Select(t => CreateModuleInfo(t));

            var moduleCatalog = Container.Resolve<IModuleCatalog>();
            var moduleManager = Container.Resolve<IModuleManager>();

            foreach (var moduleInfo in moduleInfos)
            {
                if (!moduleCatalog.Modules.ToList().Exists(p => p.ModuleName == moduleInfo.ModuleName))
                {
                    moduleCatalog.AddModule(moduleInfo);
                    ApplicationService.Instance.InvokeUIAction(() =>
                    {
                        try
                        {
                            App.RegisteredUserUnHandledException = () =>
                            {
                                module.IsLoaded = false;
                                module.IsActive = false;
                                module.IsLoadedProperly = false;
                                LoggingService.Instance.Error($"Failed to load module {module.Name}", new Exception(""));

                                App.RegisteredUserUnHandledException = null;
                            };

                            moduleManager.LoadModule(moduleInfo.ModuleName);

                            module.IsLoaded = true;
                            module.IsActive = true;
                            module.IsLoadedProperly = true;

                            LoggingService.Instance.Info($"Loaded module {module.ExecutionLocation}");
                        }
                        catch (Exception e)
                        {
                            module.IsLoaded = false;
                            module.IsActive = false;
                            module.IsLoadedProperly = false;
                            LoggingService.Instance.Error($"Failed to load module {module.Name}", e);
                        }
                    });
                }
            }
        }

        public static void EnableModule(string moduleName)
        {
            if (Container == null)
            {
                throw new FileNotFoundException($"ModuleLoader.Container property must be setup!");
            }

            var module = Repositories.RepositoryLocal.ModuleList.FirstOrDefault(p => p.Name == moduleName);
            if (module != null)
            {
                try
                {
                    LoadModule(module);
                    module.IsActive = true;
                }
                catch (Exception e)
                {
                    module.IsActive = false;
                    LoggingService.Instance.Error($"Failed to load module '{module.Name}'", e);
                }
            }
        }

        public static void DisableModule(string moduleName)
        {
            if (Container == null)
            {
                throw new FileNotFoundException($"ModuleLoader.Container property must be setup!");
            }

            var module = Repositories.RepositoryLocal.ModuleList.FirstOrDefault(p => p.Name == moduleName);
            if (module != null)
            {
                module.IsActive = false;
            }
        }

        public static void MarkModuleAsDeleted(string moduleName, string version)
        {
            if (Container == null)
            {
                throw new FileNotFoundException($"ModuleLoader.Container property must be setup!");
            }

            var module = Repositories.RepositoryLocal.ModuleList.FirstOrDefault(p => p.Name == moduleName);
            if (module != null)
            {
                module.MarkedUninstalledVersions.Add(version);
                module.Version = version;
            }

            Repositories.Store();
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

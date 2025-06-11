using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.Views.Windows;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ModuleLoader = PowerTools.Helpers.ModuleLoader;

namespace PowerTools.ViewModels
{
    public class ModuleListViewModel : BindableBase
    {
        private ObservableCollection<ToolModule> _allModules;
        private ObservableCollection<ToolModule> _modules;

        public ObservableCollection<ToolModule> Modules
        {
            get => _modules;
            set
            {
                _modules = value;
                RaisePropertyChanged("Modules");
            }
        }

        private ToolModule _selectedModule;

        public ToolModule SelectedModule
        {
            get => _selectedModule;
            set
            {
                _selectedModule = value;
                RaisePropertyChanged("SelectedModule");
            }
        }

        private string _loadingText1;
        public string LoadingText1
        {
            get => IsLoadedModules ? string.Empty : _loadingText1;
            set => _loadingText1 = value;
        }

        private string _loadingText2;
        public string LoadingText2
        {
            get => IsLoadedModules ? string.Empty : _loadingText2;
            set => _loadingText2 = value;
        }

        private bool _isLoadedModules;

        public bool IsLoadedModules
        {
            get => _isLoadedModules;
            set
            {
                _isLoadedModules = value;
                RaisePropertyChanged("IsLoadedModules");
                RaisePropertyChanged("LoadingText1");
                RaisePropertyChanged("LoadingText2");
            }
        }

        private string _searchingText;
        private readonly IContainerProvider _container;

        public string SearchingText
        {
            get => _searchingText;
            set
            {
                _searchingText = value;
                RaisePropertyChanged("SearchingText");
            }
        }

        public ICommand CmdSearchModule { get; set; }
        public ICommand CmdShowSettings { get; set; }
        public ICommand CmdRefreshModules { get; set; }
        public ICommand CmdExecuteModule { get; set; }
        public ICommand CmdInstallModule { get; set; }

        public ModuleListViewModel(IContainerProvider container)
        {
            this._container = container;

            CmdSearchModule = new DelegateCommand(OnCmdSearchModule);
            CmdShowSettings = new DelegateCommand(OnCmdShowSettings);
            CmdRefreshModules = new DelegateCommand(OnCmdRefreshModules);
            CmdExecuteModule = new DelegateCommand(OnCmdExecuteModule);
            CmdInstallModule = new DelegateCommand(OnCmdInstallModule);

            LoadingText1 = "Loading modules...";
            LoadingText2 = "Please check the configurations to make sure you're using the correct repository";

            OnLoadLocalModules();
        }

        private void OnCmdInstallModule()
        {
            if (SelectedModule == null)
            {
                MessageBox.Show("Please select a module before installing!");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedModule.Version))
            {
                MessageBox.Show("Please select a version before installing!");
                return;
            }

            Task.Run(() =>
            {
                LoggingService.Instance.Clear();

                LoggingService.Instance.Info($"Installing module {SelectedModule.Name}...");
                DownloadModule(SelectedModule);
            });
        }

        private void OnCmdExecuteModule()
        {
            if (SelectedModule == null)
            {
                MessageBox.Show("Please select a module before installing!");
                return;
            }

            ExecuteModule(SelectedModule);
        }

        private void DownloadModule(ToolModule module)
        {
            LoggingService.Instance.Info($"Downloading... module{module.Name}");

            var remoteRepositoryPath = ModuleGlobalSettings.Instance.RepositoryRemote;
            if (!Directory.Exists(remoteRepositoryPath))
            {
                MessageBox.Show($"Cannot find the remote repository path {remoteRepositoryPath}");
                LoggingService.Instance.Clear();
                return;
            }

            var moduleName =
                $"{module.Name}_v{module.Version}.{Constants.ModuleExtensionFileName}";
            var remoteModulePath = Path.Combine(remoteRepositoryPath, moduleName);

            if (!File.Exists(remoteModulePath))
            {
                MessageBox.Show($"Cannot find the module from the remote repository within specific version {remoteModulePath}");
                LoggingService.Instance.Clear();
                return;
            }

            var tempFolder = GetOrCreateTempFolder();
            var tempModuleFile = Path.Combine(tempFolder, moduleName);

            // Copy the module to local
            File.Copy(remoteModulePath, tempModuleFile);

            // Create package folder in local
            var localModuleFolder = ModuleGlobalSettings.Instance.RepositoryLocal;
            localModuleFolder = Path.Combine(localModuleFolder, module.Name);
            localModuleFolder = Path.Combine(localModuleFolder, module.Version);

            Directory.CreateDirectory(localModuleFolder);

            try
            {
                ZipFile.ExtractToDirectory(tempModuleFile, localModuleFolder);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Info($"Cannot extract the module {tempModuleFile} to {localModuleFolder}");
                return;
            }

            //var tempVersion = module.Version;
            //module.Version = string.Empty;
            module.Version =  module.Version;

            LoggingService.Instance.Info($"Done! Downloaded module {module.Name}");
        }

        private string GetOrCreateTempFolder()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        private void ExecuteModule(ToolModule module)
        {
            ModuleGlobalSettings.Instance.CurrentModule = module;

            try
            {
                // Check if the module is loaded successfully
                // If the module is not initialized, just load it

                var regionManager = _container.Resolve<IRegionManager>();
                var moduleName = ModuleGlobalSettings.Instance.CurrentModule.Name;
                var regionName = Constants.ModuleRegionName;

                //if (regionManager.Regions.ContainsRegionWithName(regionName))
                //{
                //    var selectedTypeView = ModuleViewSelectionHelper.GetView(moduleName);
                //    if (selectedTypeView == null)
                //    {
                //        ModuleLoader.Instance.LoadModule(_container, ModuleGlobalSettings.Instance.CurrentModule);
                //        selectedTypeView = ModuleViewSelectionHelper.GetView(moduleName);
                //    }

                //    if (selectedTypeView != null)
                //    {
                //        var view = regionManager.Regions[Constants.ModuleRegionName].Views
                //            .FirstOrDefault(p => p.GetType().FullName == selectedTypeView.FullName);

                //        regionManager.Regions[Constants.ModuleRegionName].Activate(view);
                //    }
                //    else
                //    {
                //        regionManager.Regions[Constants.ModuleRegionName].Activate(null);
                //    }
                //}
                //else
                //{
                //    ModuleLoader.Instance.LoadModule(_container, ModuleGlobalSettings.Instance.CurrentModule);
                //}

                //ViewNavigator.Instance.NavigateToModuleView(_container);
                RepositoryLoader.Instance.Store();
                //LoggingService.Instance.Clear();

                var actionResult = MessageBox.Show("The tool will be restarted to activate the module", "Information",
                    MessageBoxButton.YesNo);

                if (actionResult == MessageBoxResult.Yes)
                {
                    RepositoryLoader.Instance.Store();
                    ApplicationService.Instance.Restart();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void RestartApp(int pid)
        {
            // Wait for the process to terminate
            Process process = null;
            try
            {
                process = Process.GetProcessById(pid);
                process.WaitForExit(1000);

                Process.GetCurrentProcess().Close();
            }
            catch (ArgumentException ex)
            {
                // ArgumentException to indicate that the 
                // process doesn't exist?   LAME!!
            }
        }

        private void OnCmdRefreshModules()
        {
            SetLoadModulesStatus(false);

            Task.Run(() =>
            {
                LoggingService.Instance.Info("Loading module list from remote repository!");

                RepositoryLoader.Instance.Refresh();
                RepositoryLoader.Instance.Store();

                SetLoadModulesStatus(true, RepositoryLoader.Instance.LocalRepository.ModuleList);

                LoggingService.Instance.Clear();
            });
        }

        private void OnLoadLocalModules()
        {
            SetLoadModulesStatus(false);

            Task.Run(() =>
            {
                LoggingService.Instance.Info("Loading module list from local repository!");

                RepositoryLoader.Instance.LoadLocalRepository();
                SelectedModule = ModuleGlobalSettings.Instance.CurrentModule;

                SetLoadModulesStatus(true, RepositoryLoader.Instance.LocalRepository.ModuleList);

                LoggingService.Instance.Clear();
            });
        }

        private void SetLoadModulesStatus(bool isLoaded, IEnumerable<ToolModule> modules = null)
        {
            if (!isLoaded)
            {
                _allModules = new ObservableCollection<ToolModule>();
                Modules = new ObservableCollection<ToolModule>(_allModules);
                IsLoadedModules = false;
            }
            else
            {
                if (modules != null && modules.Any())
                {
                    _allModules = new ObservableCollection<ToolModule>(modules);
                    Modules = new ObservableCollection<ToolModule>(_allModules);
                    IsLoadedModules = true;
                }
            }
        }

        private void OnCmdShowSettings()
        {
            new ModuleSettings().ShowDialog();

            Modules = new ObservableCollection<ToolModule>();
            IsLoadedModules = false;
            Task.Run(OnCmdRefreshModules);
        }

        private void OnCmdSearchModule()
        {
            Modules = new ObservableCollection<ToolModule>(_allModules.Where(p => string.IsNullOrEmpty(SearchingText) || p.Name.ToLower().Contains(SearchingText.ToLower())));
        }
    }
}

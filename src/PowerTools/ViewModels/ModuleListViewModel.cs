using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.Jobs;
using PowerTools.Models;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
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

namespace PowerTools.ViewModels
{
    public class ModuleListViewModel : BindableBase
    {
        private List<ToolModule> _allModules => Repositories.RepositoryLocal.ModuleList;
        public ObservableCollection<ToolModule> ModuleList => new(_allModules.Where(p => !p.IsInstalled && (string.IsNullOrEmpty(SearchingText) || p.Name.ToLower().Contains(SearchingText.ToLower()))));
        public ObservableCollection<ToolModule> InstalledModuleList => new(_allModules.Where(p => p.IsInstalled && (string.IsNullOrEmpty(SearchingText) || p.Name.ToLower().Contains(SearchingText.ToLower()))));

        private ToolModule _selectedModule;

        public ToolModule SelectedModule
        {
            get => _selectedModule;
            set
            {
                _selectedModule = value;
                RaisePropertyChanged();
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
                RaisePropertyChanged();
                RaisePropertyChanged("ModuleList");
                RaisePropertyChanged("InstalledModuleList");
                RaisePropertyChanged("AdditionalInstalledInfo");
                RaisePropertyChanged("AdditionalRecommendedInfo");
            }
        }

        public string AdditionalInstalledInfo => $"({InstalledModuleList.Count})";
        public string AdditionalRecommendedInfo => $"({ModuleList.Count})";

        public ICommand CmdRefreshModules { get; set; }
        public ICommand CmdInstallModule { get; set; }
        public ICommand CmdUninstallModule { get; set; }
        public ICommand CmdNavigateRepoLink { get; set; }
        public ICommand CmdDisableModule { get; set; }
        public ICommand CmdEnableModule { get; set; }

        private IDialogService _dialogService;

        public ModuleListViewModel(IContainerProvider container, IDialogService dialogService)
        {
            this._container = container;
            this._dialogService = dialogService;

            CmdRefreshModules = new DelegateCommand(OnCmdRefreshModules);
            CmdInstallModule = new DelegateCommand(OnCmdInstallModule);
            CmdUninstallModule = new DelegateCommand(OnCmdUninstallModule);
            CmdNavigateRepoLink = new DelegateCommand(OnCmdNavigateRepoLink);
            CmdDisableModule = new DelegateCommand<string>(OnCmdDisableModule);
            CmdEnableModule = new DelegateCommand<string>(OnCmdEnableModule);

            Repositories.RepositoryLocal.Load();
            ModuleGlobalSettings.Instance.ResetWindowSettings();
        }

        private void OnCmdEnableModule(string moduleName)
        {
            var module = _allModules.FirstOrDefault(p => p.Name == moduleName);
            if (module != null)
            {
                module.IsActive = true;
                ModuleLoader.EnableModule(moduleName);
                Repositories.NotifyRepositoriesChanged();
                Repositories.Store();
            }
        }

        private void OnCmdDisableModule(string moduleName)
        {
            var module = _allModules.FirstOrDefault(p => p.Name == moduleName);
            if (module != null)
            {
                module.IsActive = false;
                ModuleLoader.DisableModule(moduleName);
                Repositories.Store();
            }
        }

        private void OnCmdNavigateRepoLink()
        {
            if (SelectedModule == null || string.IsNullOrWhiteSpace(SelectedModule.RepoLink) || string.IsNullOrEmpty(SelectedModule.RepoLink))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(SelectedModule.RepoLink) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open the repository link: {ex.Message}");
            }
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
                LoggingService.Instance.Info($"Installing module {SelectedModule.Name}...");

                var moduleName = SelectedModule.Name;
                DownloadModule(SelectedModule);

                RaisePropertyChanged("ModuleList");
                RaisePropertyChanged("InstalledModuleList");
                RaisePropertyChanged("AdditionalInstalledInfo");
                RaisePropertyChanged("AdditionalRecommendedInfo");

                var installedModule = InstalledModuleList.FirstOrDefault(p => p.Name == moduleName);
                if (installedModule != null)
                {
                    SelectedModule = installedModule;
                }
            });
        }

        private void OnCmdUninstallModule()
        {
            if (SelectedModule == null)
            {
                MessageBox.Show("Please select a module before uninstalling!");
                return;
            }

            if (string.IsNullOrWhiteSpace(SelectedModule.Version))
            {
                MessageBox.Show("Please select a version before uninstalling!");
                return;
            }

            Task.Run(() =>
            {
                LoggingService.Instance.Info($"Uninstalling module {SelectedModule.Name}...");
                ModuleLoader.DisableModule(SelectedModule.Name);
                ModuleLoader.MarkModuleAsDeleted(SelectedModule.Name, SelectedModule.Version);

                ApplicationService.Instance.InvokeUIAction(() =>
                {
                    ApplicationService.Instance.MessageBox("The application will be restarted to apply changes", "Notification");
                    ApplicationService.Instance.Restart();
                });
            });
        }

        private void DownloadModule(ToolModule module)
        {
            LoggingService.Instance.Info($"Downloading... module{module.Name}");

            var remoteRepositoryPath = ModuleGlobalSettings.Instance.RepositoryRemote;
            if (!File.Exists(remoteRepositoryPath))
            {
                MessageBox.Show($"Cannot find the remote repository path {remoteRepositoryPath}");
                return;
            }

            var remoteDirectory = Path.GetDirectoryName(remoteRepositoryPath);
            if (!Directory.Exists(remoteDirectory))
            {
                MessageBox.Show($"Cannot find the remote repository path {remoteDirectory}");
                return;
            }

            var downloadModuleName =
                $"{module.Name}-{module.Version}.{Constants.ModuleExtensionFileName}";
            var remoteModulePath = Path.Combine(remoteDirectory, downloadModuleName);

            if (!File.Exists(remoteModulePath))
            {
                MessageBox.Show(
                    $"Cannot find the module from the remote repository within specific version {remoteModulePath}");
                return;
            }

            module.VersionUpdateStatus = VersionUpdateStatus.Updating;
            var tempFolder = ApplicationService.Instance.GetOrCreateTempFolder();
            var tempModuleFile = Path.Combine(tempFolder, downloadModuleName);

            // Create package folder in local
            var localModuleFolder = Path.Combine(ModuleGlobalSettings.Instance.RepositoryLocalName, $"{module.Name}-{module.Version}");

            try
            {
                // Copy the module to local
                File.Copy(remoteModulePath, tempModuleFile);
                Directory.CreateDirectory(localModuleFolder);
                ZipFile.ExtractToDirectory(tempModuleFile, localModuleFolder);
            }
            catch (Exception e)
            {
                LoggingService.Instance.Info($"Cannot extract the module {tempModuleFile} to {localModuleFolder}");
                module.VersionUpdateStatus = VersionUpdateStatus.CheckForUpdates;
                return;
            }

            module.VersionUpdateStatus = VersionUpdateStatus.Done;

            var foundModule = Repositories.RepositoryLocal.ModuleList.First(p => p.Name == module.Name);
            foundModule.Version = module.Version;

            try
            {
                ModuleLoader.EnableModule(module.Name);
                foundModule.IsActive = true;
                foundModule.IsLoadedProperly = true;
            }
            catch (Exception e)
            {
                foundModule.IsActive = false;
                foundModule.IsLoadedProperly = false;

                LoggingService.Instance.Info($"Cannot extract the module {tempModuleFile} to {localModuleFolder}");
            }

            RaisePropertyChanged("SelectedModule");
            Repositories.NotifyRepositoriesChanged();
            Repositories.Store();
            LoggingService.Instance.Info($"Done! Downloaded module {module.Name}");
        }

        private void OnCmdRefreshModules()
        {
            TaskExecution.Instance.RunOnceAsync("Refresh Modules", () =>
            {
                LoggingService.Instance.Info("Loading module list from remote repository!");

                Repositories.Refresh();
                Repositories.NotifyRepositoriesChanged();
                Repositories.Store();

                RaisePropertyChanged("ModuleList");
                RaisePropertyChanged("SelectedModule");
                RaisePropertyChanged("InstalledModuleList");
                RaisePropertyChanged("AdditionalInstalledInfo");
                RaisePropertyChanged("AdditionalRecommendedInfo");

                BackgroundJobs.New()
                    .AddJob(new CheckModuleVersions("Check Modules version when refreshing",0))
                    .Process();
            });
        }
    }
}

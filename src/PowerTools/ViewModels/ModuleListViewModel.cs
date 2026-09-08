using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.Jobs;
using PowerTools.Utils;
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
                MessageBox.Show("Please select a module before install!");
                return;
            }

            var clonedModule = (ToolModule)SelectedModule.Clone();
            var taskName = $"Check for updates - Module {SelectedModule.Name}";

            switch (SelectedModule.VersionUpdateStatus)
            {
                case VersionUpdateStatus.NoUpdates:
                    SelectedModule.VersionUpdateStatus = VersionUpdateStatus.CheckForUpdates;
                    TaskExecution.Instance.RunOnceAsync(taskName, (taskReport) => CheckForUpdate(taskReport, clonedModule));
                    break;

                case VersionUpdateStatus.HasNewVersion:
                    SelectedModule.VersionUpdateStatus = VersionUpdateStatus.Updating;
                    TaskExecution.Instance.RunOnceAsync(taskName, (taskReport) => InstallLatestVersion(taskReport, clonedModule));
                    break;

                case VersionUpdateStatus.Done:
                    ApplicationService.Instance.Restart();
                    break;

                default: break;
            }
        }

        private void InstallLatestVersion(ITaskReport taskReport, ToolModule module)
        {
            LoggingService.Instance.Info($"Installing module {module.Name}...");

            var toolModule = Repositories.RepositoryLocal.ModuleList.FirstOrDefault(p => p.Name == module.Name);
            if (toolModule == null)
            {
                return;
            }

            module.Version = module.NewVersion;
            if (module.IsInstalled)
            {
                ApplicationService.Instance.InvokeUIAction(() =>
                {
                    toolModule.Version = toolModule.NewVersion;
                    toolModule.NewVersion = null;
                    toolModule.VersionUpdateStatus = VersionUpdateStatus.NoUpdates;

                    RaisePropertyChanged("ModuleList");
                    RaisePropertyChanged("InstalledModuleList");
                    RaisePropertyChanged("AdditionalInstalledInfo");
                    RaisePropertyChanged("AdditionalRecommendedInfo");

                    var installedModule = InstalledModuleList.FirstOrDefault(p => p.Name == module.Name);
                    if (installedModule != null)
                    {
                        SelectedModule = installedModule;
                    }

                    OnCmdEnableModule(toolModule.Name);
                });
            }
            else
            {
                if (module.RepoType == "github")
                {
                    DownloadModuleFromGithub(module);
                }
                else
                {
                    DownloadModule(module);
                }
            }
        }

        private void DownloadModuleFromGithub(ToolModule module)
        {
            if (string.IsNullOrEmpty(module.NewVersion))
            {
                return;
            }

            LoggingService.Instance.Info($"Downloading... module{module.Name} - {module.NewVersion}");

            try
            {
                if (module.TokenRequired && string.IsNullOrEmpty(module.Token))
                {
                    LoggingService.Instance.Info($"Token is required for module {module.Name} but is missing.");
                    return;
                }

                var latestRelease = GithubProvider.GetLatestRelease(module.OwnerName, module.RepoName, module.Token).Result;
                var localReleaseAssets = GithubProvider.DownloadReleaseAssets(module.OwnerName, module.RepoName, module.Token, latestRelease).Result;

                if (!File.Exists(localReleaseAssets.ReleasePath))
                {
                    LoggingService.Instance.Info($"Module {module.Name} is not downloaded!");
                    return;
                }

                var _7zPath = Get7zExecutionPath();
                var tempExtractedNewVersion = ApplicationService.Instance.GetOrCreateTempFolder();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _7zPath,
                        Arguments = $"x \"{localReleaseAssets.ReleasePath}\" -o\"{tempExtractedNewVersion}\" -y",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                process.Start();

                process.WaitForExit();
                process.Close();
                process.Dispose();

                if (File.Exists(localReleaseAssets.ChangelogsPath))
                {
                    var destChangelogsFile = Path.Combine(tempExtractedNewVersion, "Changelogs.md");
                    File.Copy(localReleaseAssets.ChangelogsPath, destChangelogsFile, true);
                }

                if (File.Exists(localReleaseAssets.ReadmePath))
                {
                    var destReadmeFile = Path.Combine(tempExtractedNewVersion, "Readme.md");
                    File.Copy(localReleaseAssets.ReadmePath, destReadmeFile, true);
                }

                module.Version = module.NewVersion;

                CopyDirectory(tempExtractedNewVersion, module.ModuleLocation);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error($"Failed to download module {module.Name}: {ex.Message}", ex);
            }
        }
        private void CopyDirectory(string sourceDir, string destinationDir, bool overwrite = true)
        {
            // 1. Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            // 2. Create the destination directory if it doesn't exist
            Directory.CreateDirectory(destinationDir);

            // 3. Copy all files in the current directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite);
            }

            // 4. Recursively copy all subdirectories
            foreach (DirectoryInfo subDir in dir.GetDirectories())
            {
                string targetSubDirPath = Path.Combine(destinationDir, subDir.Name);
                CopyDirectory(subDir.FullName, targetSubDirPath, overwrite);
            }
        }
        private string Get7zExecutionPath()
        {
            var seventZipPath = ModuleGlobalSettings.Instance.GetModuleConfigurationsByKey("7zExecutionPath");
            if (string.IsNullOrEmpty(seventZipPath))
            {
                var executionFolder = Path.GetDirectoryName(typeof(PowerTools.App).Assembly.Location);
                seventZipPath = Path.Combine(executionFolder, @"tools\7z\7z.exe");

                ModuleGlobalSettings.Instance.SaveModuleConfigurationsByKey("7zExecutionPath", seventZipPath);
            }

            return seventZipPath;
        }

        private void CheckForUpdate(ITaskReport taskReport, ToolModule module)
        {
            taskReport.SetDescription($"Checking versions of module {module.DisplayName}");

            var toolModule = Repositories.RepositoryLocal.ModuleList.FirstOrDefault(p => p.Name == module.Name);
            if (toolModule == null)
            {
                return;
            }

            try
            {
                if (module.TokenRequired && string.IsNullOrEmpty(module.Token))
                {
                    toolModule.VersionUpdateStatus = VersionUpdateStatus.NoUpdates;
                    return;
                }

                var latestRelease = GithubProvider.GetLatestRelease(module.OwnerName, module.RepoName, module.Token).Result;
                if (latestRelease == null)
                {
                    toolModule.VersionUpdateStatus = VersionUpdateStatus.NoUpdates;
                    return;
                }

                var newVersion = latestRelease.TagName.TrimStart('v');
                if (newVersion.GetVersionValue() <= module.Version.GetVersionValue())
                {
                    toolModule.VersionUpdateStatus = VersionUpdateStatus.NoUpdates;
                    return;
                }

                toolModule.VersionUpdateStatus = VersionUpdateStatus.HasNewVersion;
                toolModule.NewVersion = newVersion;
            }
            catch (Exception ex)
            {
                taskReport.SetDescription($"Failed to check versions of module {module.Name}: {ex.Message}");
                toolModule.VersionUpdateStatus = VersionUpdateStatus.NoUpdates;
            }
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
                    .AddJob(new CheckModuleVersions("Check Modules version when refreshing", 0))
                    .Process();
            });
        }
    }
}

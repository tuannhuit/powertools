using Octokit;
using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Input;

namespace PowerTools.Models
{
    public class AppVersion : BindableBase
    {
        private readonly object _lockObject = new object();
        private VersionUpdateStatus _status;
        private List<string> _versionList;
        private string _newVersion;
        private string _onlineNewVersionFilePath;
        private string _downloadedNewVersionFilePath;

        public VersionUpdateStatus Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
                RaisePropertyChanged(nameof(StatusAsString));
            }
        }

        public string StatusAsString
        {
            get
            {
                switch (Status)
                {
                    case VersionUpdateStatus.NoUpdates:
                        return "Check for Updates";
                    case VersionUpdateStatus.CheckForUpdates:
                        return "Checking new version";
                    case VersionUpdateStatus.HasNewVersion:
                        return $"Install v{NewVersion}";
                    case VersionUpdateStatus.Updating:
                        return "Downloading ...";
                    case VersionUpdateStatus.Done:
                        return "Restart";
                    default:
                        return "Unknown";
                }
            }
        }

        public string CurrentVersion => App.Version;

        public string NewVersion
        {
            get => _newVersion;
            private set
            {
                _newVersion = value;
                RaisePropertyChanged(nameof(NewVersion));
                RaisePropertyChanged(nameof(NewVersionUpdateAsString));
            }
        }

        public string NewVersionUpdateAsString => $"Update current version v{CurrentVersion} to v{NewVersion}";

        public ICommand CmdUpdateToLatestVersion { get; set; }

        public AppVersion()
        {
            _versionList = new List<string>();
            Status = VersionUpdateStatus.NoUpdates;
            CmdUpdateToLatestVersion = new DelegateCommand(OnCmdUpdateToLatestVersion);
        }

        private void OnCmdUpdateToLatestVersion()
        {
            // Start downloading latest version of the application
            if (Status == VersionUpdateStatus.NoUpdates)
            {
                Status = VersionUpdateStatus.CheckForUpdates;
                TaskExecution.Instance.RunAsync(OnCheckForUpdate);
            }
            else if (Status == VersionUpdateStatus.HasNewVersion)
            {
                Status = VersionUpdateStatus.Updating;
                TaskExecution.Instance.RunAsync(OnDownloadNewVersion);
            }
            else if (Status == VersionUpdateStatus.Done)
            {
                ApplicationService.Instance.Shutdown();
            }
        }

        private async void OnDownloadNewVersion(ITaskReport obj)
        {
            if (string.IsNullOrEmpty(NewVersion))
            {
                return;
            }

            if (string.IsNullOrEmpty(_onlineNewVersionFilePath))
            {
                return;
            }

            var localFilePath = $"{ApplicationService.Instance.GetOrCreateTempFolder()}\\PowerTools.v{NewVersion}.zip";
            var httpClient = new HttpClient();

            try
            {
                using (var downloadStream = await httpClient.GetStreamAsync(_onlineNewVersionFilePath))
                {
                    using (var fileStream = new FileStream(localFilePath, System.IO.FileMode.Create,
                               System.IO.FileAccess.Write, System.IO.FileShare.None))
                    {
                        await downloadStream.CopyToAsync(fileStream);

                        _downloadedNewVersionFilePath = localFilePath;
                        LoggingService.Instance.Info($"Downloaded new version to {localFilePath}");
                    }
                }

                Status = VersionUpdateStatus.Done;
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error("Failed to download new version", e);
                Status = VersionUpdateStatus.HasNewVersion;
            }
        }

        private bool ValidateVersions(IEnumerable<string> versionList)
        {
            lock (_lockObject)
            {
                if (!versionList.Any())
                {
                    return false;
                }

                _versionList = new List<string>(versionList);

                var orderedList = _versionList.OrderBy(p => p.GetVersionValue());
                var latestVersion = orderedList.Last();

                if (latestVersion.GetVersionValue() > CurrentVersion.GetVersionValue())
                {
                    NewVersion = latestVersion;
                    Status = VersionUpdateStatus.HasNewVersion;

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Start a new Task to check for new versions of the application.
        /// This method will run asynchronously and update the VersionUpdateStatus property accordingly.
        /// </summary>
        public void CheckForUpdate()
        {
            TaskExecution.Instance.RunOnceAsync(
                "Check Ver sions",
                OnCheckForUpdate,
                null,
                null,
                null,
                false,
                60 * 60 * 1000,
                false);
        }

        private void OnCheckForUpdate(ITaskReport taskReport)
        {
            taskReport.SetDescription($"Start checking versions of {App.AppName}");

            var client = new GitHubClient(new ProductHeaderValue(App.AppName));
            try
            {
                var releases = client.Repository.Release.GetAll(App.OwnerName, App.RepoName).Result;
                if (releases.Count == 0)
                {
                    LoggingService.Instance.Info($"Found no version of {App.AppName}");
                    return;
                }

                var versions = releases.Select(p => p.TagName.TrimStart('v'));

                ValidateVersions(versions);

                if (Status == VersionUpdateStatus.HasNewVersion)
                {
                    FindDownloadNewVersionLink(releases);
                    taskReport.SetDescription($"Found new version v{NewVersion} for {App.AppName}");
                }
                else
                {
                    Status = VersionUpdateStatus.NoUpdates;
                    taskReport.SetDescription($"No updates found for {App.AppName}");
                }
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error("Failed to check versions", e);
                Status = VersionUpdateStatus.NoUpdates;
            }
        }

        private void FindDownloadNewVersionLink(IReadOnlyList<Release> releases)
        {
            lock (_lockObject)
            {
                if (Status == VersionUpdateStatus.HasNewVersion)
                {
                    var asset = releases.FirstOrDefault(p => p.TagName == NewVersion);
                    if (asset != null)
                    {
                        var validAsset = asset.Assets.FirstOrDefault(p => p.Name == $"PowerTools.v{NewVersion}.zip");
                        if (validAsset != null)
                        {
                            _onlineNewVersionFilePath = validAsset.BrowserDownloadUrl;
                        }
                    }
                }
            }
        }

        public void InstallNewVersion()
        {
            if (Status == VersionUpdateStatus.Done && !string.IsNullOrEmpty(_downloadedNewVersionFilePath))
            {
                ApplicationService.Instance.Busy("Preparing for new version...");

                var currentFolder = Path.GetDirectoryName(typeof(PowerTools.App).Assembly.Location);
                var tempExtractedNewVersion = ApplicationService.Instance.GetOrCreateTempFolder();

                ApplicationService.Instance.Busy("Extracting");
                var _7zPath = Get7zExecutionPath();

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _7zPath,
                        Arguments = $"x \"{_downloadedNewVersionFilePath}\" -o\"{tempExtractedNewVersion}\" -y",
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

                ApplicationService.Instance.Busy("Starting VersionInstaller...");

                // Copy VersionInstaller to another temporary folder
                var versionInstallerFolder = ApplicationService.Instance.GetOrCreateTempFolder();
                var versionInstallerExtractedFolder = Path.Combine(tempExtractedNewVersion, "tools\\VersionInstaller");

                try
                {
                    CopyDirectory(versionInstallerExtractedFolder, versionInstallerFolder);
                }
                catch (Exception e)
                {
                    LoggingService.Instance.Error("Failed to copy VersionInstaller directory", e);
                    ApplicationService.Instance.MessageBox("Failed to install new version: Not found VersionInstaller");
                    return;
                }   

                var versionInstallerExecutionPath = Path.Combine(versionInstallerFolder, "PowerTools.Apps.VersionInstaller.exe");

                var versionInstallerProcess = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = versionInstallerExecutionPath,
                        Arguments = $"\"{tempExtractedNewVersion}\" \"{currentFolder}\"",
                        UseShellExecute = true
                    }
                };

                versionInstallerProcess.Start();
                versionInstallerProcess.Dispose();
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
    }
}

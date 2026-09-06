using Octokit;
using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace PowerTools.Models
{
    public class AppVersion : BindableBase
    {
        private readonly object _lockObject = new object();
        private VersionUpdateStatus _status;
        private ReleaseInformation _releaseInformation;
        private List<string> _versionList;
        private string _newVersion;
        private Release _latestRelease;
        private string _downloadedNewVersionFilePath;
        private string _downloadedReadmeFilePath;
        private string _downloadedChangelogsFilePath;

        public VersionUpdateStatus Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
                //RaisePropertyChanged(nameof(StatusAsString));
            }
        }

        //public string StatusAsString
        //{
        //    get
        //    {
        //        switch (Status)
        //        {
        //            case VersionUpdateStatus.NoUpdates:
        //                return "Check for Updates";
        //            case VersionUpdateStatus.CheckForUpdates:
        //                return "Checking new version";
        //            case VersionUpdateStatus.HasNewVersion:
        //                return $"Install v{NewVersion}";
        //            case VersionUpdateStatus.Updating:
        //                return "Downloading ...";
        //            case VersionUpdateStatus.Done:
        //                return "Restart";
        //            default:
        //                return "Unknown";
        //        }
        //    }
        //}

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
        public ICommand CmdShowReleaseInformation { get; set; }

        public AppVersion()
        {
            _versionList = new List<string>();
            Status = VersionUpdateStatus.NoUpdates;
            CmdUpdateToLatestVersion = new DelegateCommand(OnCmdUpdateToLatestVersion);
            CmdShowReleaseInformation = new DelegateCommand(OnCmdShowReleaseInformation);
        }

        private void OnCmdShowReleaseInformation()
        {
            if (_releaseInformation == null)
            {
                _releaseInformation = new ReleaseInformation();

                var currentFolder = ApplicationService.Instance.GetCurrentExecutionPath();
                var changelogsFile = Path.Combine(currentFolder, "Changelogs.md");
                var detailsFile = Path.Combine(currentFolder, "Readme.md");

                if (File.Exists(changelogsFile))
                {
                    _releaseInformation.ChangeLogs = File.ReadAllText(changelogsFile);
                }

                if (File.Exists(detailsFile))
                {
                    _releaseInformation.Details = File.ReadAllText(detailsFile);
                }
            }

            var actions = new List<DialogAction>();

            ApplicationService.Instance.ShowDialog<Views.UserControls.ReleaseInformation>(
                $"Release Information v{CurrentVersion}",
                _releaseInformation,
                null,
                actions,
                850,
                400);
        }

        private void OnCmdUpdateToLatestVersion()
        {
            // Start downloading latest version of the application
            if (Status == VersionUpdateStatus.NoUpdates)
            {
                Status = VersionUpdateStatus.CheckForUpdates;
                TaskExecution.Instance.RunAsync(CheckForUpdate);
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

            if (_latestRelease == null)
            {
                return;
            }

            LocalReleaseAssets localReleaseAssets = null;

            try
            {
                localReleaseAssets = GithubProvider.DownloadReleaseAssets(App.OwnerName, App.RepoName, null, _latestRelease).Result;

                if (File.Exists(localReleaseAssets.ReleasePath))
                {
                    _downloadedNewVersionFilePath = localReleaseAssets.ReleasePath;
                    Status = VersionUpdateStatus.Done;
                    LoggingService.Instance.Info($"Downloaded new version to {_downloadedNewVersionFilePath}");
                }
                else
                {
                    LoggingService.Instance.Info("Failed to download new version");
                    Status = VersionUpdateStatus.HasNewVersion;
                }

                if (File.Exists(localReleaseAssets.ChangelogsPath))
                {
                    _downloadedChangelogsFilePath = localReleaseAssets.ChangelogsPath;
                    LoggingService.Instance.Info($"Downloaded Changelogs.md to {_downloadedChangelogsFilePath}");
                }

                if (File.Exists(localReleaseAssets.ReadmePath))
                {
                    _downloadedReadmeFilePath = localReleaseAssets.ReadmePath;
                    LoggingService.Instance.Info($"Downloaded Readme.md to {_downloadedReadmeFilePath}");
                }
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error($"Failed to download latest release {_latestRelease.Name}", e);
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
        public void CheckForUpdate(ITaskReport taskReport)
        {
            taskReport.SetDescription($"Start checking versions of {App.AppName}");

            try
            {
                var latestRelease = GithubProvider.GetLatestRelease(App.OwnerName, App.RepoName, null).Result;
                if (latestRelease != null)
                {
                    var versions = new List<string> { latestRelease.TagName.TrimStart('v') };
                    ValidateVersions(versions);

                    if (Status == VersionUpdateStatus.HasNewVersion)
                    {
                        _latestRelease = latestRelease;
                        taskReport.SetDescription($"Found new version v{NewVersion} for {App.AppName}");
                    }
                    else
                    {
                        Status = VersionUpdateStatus.NoUpdates;
                        taskReport.SetDescription($"No updates found for {App.AppName}");
                    }
                }
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error("Failed to check versions", e);
                Status = VersionUpdateStatus.NoUpdates;
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

                // Copy Changelogs and Readme files to the extracted folder
                if (File.Exists(_downloadedChangelogsFilePath))
                {
                    var destChangelogsFile = Path.Combine(tempExtractedNewVersion, "Changelogs.md");
                    File.Copy(_downloadedChangelogsFilePath, destChangelogsFile, true);
                }

                if (File.Exists(_downloadedReadmeFilePath))
                {
                    var destReadmeFile = Path.Combine(tempExtractedNewVersion, "Readme.md");
                    File.Copy(_downloadedReadmeFilePath, destReadmeFile, true);
                }

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

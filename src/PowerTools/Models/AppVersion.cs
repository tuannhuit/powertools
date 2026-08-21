using Octokit;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Input;

namespace PowerTools.Models
{
    public class AppVersion : BindableBase
    {
        private VersionUpdateStatus _versionUpdateStatus;
        private List<string> _versionList;
        private string _newVersion;
        private string _onlineNewVersionFilePath;
        private string _downloadedNewVersionFilePath;

        public VersionUpdateStatus VersionUpdateStatus
        {
            get => _versionUpdateStatus;
            set
            {
                SetProperty(ref _versionUpdateStatus, value);
                RaisePropertyChanged(nameof(CanUpdateNewVersion));
                RaisePropertyChanged(nameof(CanExecutionAction));
                RaisePropertyChanged(nameof(VersionUpdateStatusString));
            }
        }

        public string VersionUpdateStatusString
        {
            get
            {
                switch (VersionUpdateStatus)
                {
                    case VersionUpdateStatus.None:
                        return "No Update";
                    case VersionUpdateStatus.HasNewVersion:
                        return "Update";
                    case VersionUpdateStatus.Updating:
                        return "Updating...";
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
            }
        }

        public bool CanUpdateNewVersion => VersionUpdateStatus == VersionUpdateStatus.HasNewVersion || VersionUpdateStatus == VersionUpdateStatus.Updating || VersionUpdateStatus == VersionUpdateStatus.Done;
        public bool CanExecutionAction => VersionUpdateStatus == VersionUpdateStatus.HasNewVersion || VersionUpdateStatus == VersionUpdateStatus.Done;
        public string NewVersionUpdateString => $"Update to {NewVersion}";

        public ICommand CmdUpdateToLatestVersion { get; set; }

        public AppVersion()
        {
            _versionList = new List<string>();
            VersionUpdateStatus = VersionUpdateStatus.None;
            CmdUpdateToLatestVersion = new DelegateCommand(OnCmdUpdateToLatestVersion);

            ApplicationService.Instance.Restart();
        }

        private void OnCmdUpdateToLatestVersion()
        {
            // Start downloading latest version of the application
            if (VersionUpdateStatus == VersionUpdateStatus.HasNewVersion)
            {
                VersionUpdateStatus = VersionUpdateStatus.Updating;
                TaskExecution.Instance.RunAsync(OnDownloadNewVersion);
            }
            else if (VersionUpdateStatus == VersionUpdateStatus.Done)
            {

            }
        }

        private void OnDownloadNewVersion(ITaskReport obj)
        {
            if (string.IsNullOrEmpty(NewVersion))
            {
                return;
            }

            if (string.IsNullOrEmpty(_downloadedNewVersionFilePath))
            {
                return;
            }

            TaskExecution.Instance.RunAsync(OnDownloadNewVersionFromPath);
        }

        private async void OnDownloadNewVersionFromPath(ITaskReport obj)
        {
            var localFilePath = $"{ApplicationService.Instance.GetOrCreateTempFolder()}\\PowerTools.v{NewVersion}.zip";
            var httpClient = new HttpClient();

            using (var downloadStream = await httpClient.GetStreamAsync(_onlineNewVersionFilePath))
            {
                using (var fileStream = new FileStream(localFilePath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None))
                {
                    await downloadStream.CopyToAsync(fileStream);

                    _downloadedNewVersionFilePath = localFilePath;
                    LoggingService.Instance.Info($"Downloaded new version to {localFilePath}");
                }
            }

            VersionUpdateStatus = VersionUpdateStatus.Done;
        }

        private bool ValidateVersions(IEnumerable<string> versionList)
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
                VersionUpdateStatus = VersionUpdateStatus.HasNewVersion;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Start a new Task to check for new versions of the application.
        /// This method will run asynchronously and update the VersionUpdateStatus property accordingly.
        /// </summary>
        public void CheckForUpdate()
        {
            TaskExecution.Instance.RunOnceAsync(
                "Check Versions",
                OnCheckVersions,
                null,
                null,
                null,
                false,
                5 * 60 * 1000,
                false);
        }
        private void OnCheckVersions(ITaskReport taskReport)
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

                if (VersionUpdateStatus == VersionUpdateStatus.HasNewVersion)
                {
                    var asset = releases.FirstOrDefault(p => p.TagName == NewVersion);
                    if (asset != null)
                    {
                        var validAsset = asset.Assets.FirstOrDefault(p => p.Name == $"PowerTools.v{NewVersion}.zip");
                        if (validAsset != null)
                        {
                            _downloadedNewVersionFilePath = validAsset.BrowserDownloadUrl;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error("Failed to check versions", e);
            }
        }

        public void InstallNewVersion()
        {
            if (VersionUpdateStatus == VersionUpdateStatus.Done && !string.IsNullOrEmpty(_downloadedNewVersionFilePath))
            {
                // Copy Installer into temp folder and run it
                var tempFolder = ApplicationService.Instance.GetOrCreateTempFolder();
            }
        }
    }
}

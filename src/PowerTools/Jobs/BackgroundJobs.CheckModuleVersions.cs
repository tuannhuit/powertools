using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.Utils;
using System;
using System.Linq;

namespace PowerTools.Jobs
{
    public class CheckModuleVersions : IBackgroundJob
    {
        private object _lockObject = new object();

        public string Name { get; private set; }
        public bool CanStop => false;
        public bool DoLockScreen => false;

        public bool IsStarted { get; private set; }

        public int TimeSleepInMs { get; private set; }

        public CheckModuleVersions(string name = "Check Module Versions", int timeSleepInMs = 60 * 60 * 1000)
        {
            if (timeSleepInMs < 0)
            {
                timeSleepInMs = 0;
            }

            TimeSleepInMs = timeSleepInMs;
            Name = name;
        }

        public void Process(ITaskReport taskReport)
        {
            lock (_lockObject)
            {
                IsStarted = true;
                var allModules = Repositories.RepositoryLocal.ModuleList;
                if(!allModules.Any())
                {
                    return;
                }

                foreach (var module in allModules)
                {
                    taskReport.SetDescription($"Start checking versions of {module.DisplayName}");

                    try
                    {
                        var token = ModuleGlobalSettings.Instance.GetModuleConfigurationsByKey($"github.token.{module.RepoName}");
                        if (string.IsNullOrEmpty(token))
                        {
                            token = ModuleGlobalSettings.Instance.GetModuleConfigurationsByKey("github.token");
                        }

                        var latestRelease = GithubProvider.GetLatestRelease(module.OwnerName, module.RepoName, token).Result;
                        if (latestRelease != null)
                        {
                            var newVersion = latestRelease.TagName.TrimStart('v');
                            if (newVersion.GetVersionValue() > module.Version.GetVersionValue())
                            {
                                module.VersionUpdateStatus = VersionUpdateStatus.HasNewVersion;
                                module.NewVersion = newVersion;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        LoggingService.Instance.Error($"Failed to check versions for module: {module.DisplayName}", e);
                    }
                }
            }
        }
    }
}

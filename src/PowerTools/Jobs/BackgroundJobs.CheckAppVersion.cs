using PowerTools.Core.Models;
using PowerTools.Models;
using System;

namespace PowerTools.Jobs
{
    public class CheckAppVersion : IBackgroundJob
    {
        private object _lockObject = new object();
        private AppVersion _appVersion;

        public string Name => "Check App Version";
        public bool CanStop => false;
        public bool DoLockScreen => false;

        public CheckAppVersion(AppVersion appVersion)
        {
            _appVersion = appVersion ?? throw new ArgumentNullException(nameof(appVersion));
        }

        public bool IsStarted { get; private set; }

        public int TimeSleepInMs => 60 * 60 * 1000;

        public void Process(ITaskReport taskReport)
        {
            lock (_lockObject)
            {
                IsStarted = true;
                _appVersion.CheckForUpdate(taskReport);
            }
        }
    }
}

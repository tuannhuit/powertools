using System.Collections.Generic;
using PowerTools.Core.SharedServices;

namespace PowerTools.Jobs
{
    public class BackgroundJobs
    {
        private static object _lock = new object();
        private List<IBackgroundJob> _jobs;

        public static BackgroundJobs New() => new BackgroundJobs();

        private BackgroundJobs()
        {
            _jobs = new List<IBackgroundJob>();
        }

        public BackgroundJobs AddJob(IBackgroundJob job)
        {
            _jobs.Add(job);
            return this;
        }

        public void Process()
        {
            foreach (var job in _jobs)
            {
                if (!job.IsStarted)
                {
                    string name = job.Name ?? "Background Job";
                    int timeSleepInMs = job.TimeSleepInMs < 0 ? 60 * 60 * 1000 : job.TimeSleepInMs;

                    TaskExecution.Instance.RunOnceAsync(
                        name,
                        (taskReport) => job.Process(taskReport),
                        null,
                        null,
                        null,
                        job.DoLockScreen,
                        timeSleepInMs,
                        job.CanStop);
                }
            }
        }
    }
}

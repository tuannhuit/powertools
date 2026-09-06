using PowerTools.Core.Models;

namespace PowerTools.Jobs
{
    public interface IBackgroundJob
    {
        string Name { get; }
        bool IsStarted { get; }
        bool CanStop { get; }
        bool DoLockScreen { get; }
        int TimeSleepInMs { get; }

        /// <summary>
        /// Processes the background job asynchronously.
        /// </summary>
        void Process(ITaskReport taskReport);
    }
}

using System.Threading;

namespace PowerTools.Core.Models
{
    public interface ITaskReport
    {
        CancellationTokenSource CancellationToken { get; }
        void SetProgressPercentValue(double progressValue);
        void SetDescription(string description);
    }
}

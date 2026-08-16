using PowerTools.Core.SharedServices;
using Prism.Mvvm;
using System;
using System.Threading;
using System.Windows.Input;
using Prism.Commands;

namespace PowerTools.Core.Models
{
    public class TaskInformation : BindableBase, ITaskReport
    {
        private object _lock = new object();
        private ITaskReport _taskReport;

        private string _taskName;
        public string TaskName
        {
            get => _taskName;
            private set
            {
                _taskName = value;
                RaisePropertyChanged();
            }
        }

        private string _taskDescription;
        public string TaskDescription
        {
            get => _taskDescription;
            set
            {
                _taskDescription = value;
                RaisePropertyChanged();
            }
        }

        private double _progressValue;
        public double ProgressValue
        {
            get => _progressValue;
            private set
            {
                _progressValue = value;
                RaisePropertyChanged();
            }
        }

        private TaskStatus _status;
        public TaskStatus Status
        {
            get => _status;
            private set
            {
                _status = value;
                RaisePropertyChanged();
            }
        }

        public Action<ITaskReport> Action { get; private set; }
        public ICommand CmdCancel { get; private set; }

        public CancellationTokenSource CancellationToken { get; set; }

        public TaskInformation(Action<ITaskReport> action)
            : this(string.Empty, action)
        {

        }

        public TaskInformation(string description, Action<ITaskReport> action)
            : this(Guid.NewGuid().ToString(), description, action)
        {

        }

        public TaskInformation(string taskName, string description, Action<ITaskReport> action)
        {
            TaskName = taskName;
            Action = action;
            CancellationToken = new CancellationTokenSource();
            CmdCancel = new DelegateCommand(CancelTask);

            SetDescription(description);
            SetStatus(TaskStatus.Idle);
            SetProgressPercentValue(0);
        }

        public void CancelTask()
        {
            lock (_lock)
            {
                if (Status == TaskStatus.Cancelling)
                {
                    return;
                }

                SetStatus(TaskStatus.Cancelling);
                CancellationToken.Cancel();
            }
        }

        public void SetProgressPercentValue(double progressValue)
        {
            lock (_lock)
            {
                ApplicationService.Instance.InvokeUIAction(() =>
                {
                    ProgressValue = progressValue;
                });
            }
        }

        public void SetStatus(TaskStatus status)
        {
            lock (_lock)
            {
                ApplicationService.Instance.InvokeUIAction(() =>
                {
                    Status = status;
                });
            }
        }

        public void SetDescription(string description)
        {
            lock (_lock)
            {
                ApplicationService.Instance.InvokeUIAction(() =>
                {
                    if (string.IsNullOrEmpty(description))
                    {
                        description = "empty";
                    }

                    TaskDescription = $"<{description}>";
                });
            }
        }

        public void SetTaskReport(ITaskReport taskReport)
        {
            _taskReport = taskReport;
        }
    }
}

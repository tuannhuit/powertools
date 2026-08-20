using PowerTools.Core.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using TaskStatus = PowerTools.Core.Models.TaskStatus;

namespace PowerTools.Core.SharedServices
{
    public class TaskExecution : BindableBase
    {
        public static readonly string APP_CMD = "C:\\Windows\\system32\\cmd.exe";
        private static readonly object _lock = new object();

        private Process _process;

        private List<TaskInformation> _tasks;

        public ObservableCollection<TaskInformation> Tasks => new ObservableCollection<TaskInformation>(
            _tasks.Where(t =>
                (t.Status == TaskStatus.Idle && DoShowIdle) ||
                (t.Status == TaskStatus.Error && DoShowError) ||
                (t.Status == TaskStatus.Done && DoShowDone) ||
                (t.Status == TaskStatus.Running && DoShowRunning) ||
                (t.Status == TaskStatus.Cancelling && DoShowCancelling) ||
                (!DoShowIdle && !DoShowError && !DoShowDone && !DoShowRunning && !DoShowCancelling)));

        public string TasksInformation => $"({RunningTasksCount} Running Tasks)";
        public int RunningTasksCount => _tasks.Count(p => p.Status == TaskStatus.Running);

        private bool _doShowIdle;
        public bool DoShowIdle
        {
            get => _doShowIdle;
            set
            {
                _doShowIdle = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Tasks));
            }
        }

        private bool _doShowError;
        public bool DoShowError
        {
            get => _doShowError;
            set
            {
                _doShowError = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Tasks));
            }
        }

        private bool _doShowDone;
        public bool DoShowDone
        {
            get => _doShowDone;
            set
            {
                _doShowDone = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Tasks));
            }
        }

        private bool _doShowRunning;
        public bool DoShowRunning
        {
            get => _doShowRunning;
            set
            {
                _doShowRunning = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Tasks));
            }
        }

        private bool _doShowCancelling;
        public bool DoShowCancelling
        {
            get => _doShowCancelling;
            set
            {
                _doShowCancelling = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(Tasks));
            }
        }
        public ICommand CmdCancelAllTasks { get; set; }

        #region Instance

        private static TaskExecution _instance;
        public static TaskExecution Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TaskExecution();

                return _instance;
            }
        }

        private TaskExecution()
        {
            _tasks = new List<TaskInformation>();
            DoShowRunning = true;
            CmdCancelAllTasks = new DelegateCommand(OnCancelAllTasks);

            ApplicationService.Instance.RegisterDisposableAction(OnCancelAllTasks);
        }

        private void OnCancelAllTasks()
        {
            var runningTasks = _tasks.Where(p => p.Status == TaskStatus.Running);
            foreach (var taskInformation in runningTasks)
            {
                taskInformation.CancelTask();
            }
        }

        #endregion

        public void RunAsync(Action<ITaskReport> action, Action<ITaskReport> errAction = null)
        {
            var taskInformation = new TaskInformation(action);
            taskInformation.SetStatus(TaskStatus.Running);
            taskInformation.SetProgressPercentValue(0);

            _tasks.Insert(0, taskInformation);
            RaisePropertyChanged(nameof(Tasks));
            RaisePropertyChanged(nameof(RunningTasksCount));
            RaisePropertyChanged(nameof(TasksInformation));

            Task.Run(() =>
            {
                LoggingService.Instance.Info($"Calling {taskInformation.TaskName}");

                try
                {
                    action.Invoke(taskInformation);

                    LoggingService.Instance.Info($"Done! {taskInformation.TaskName}");
                    taskInformation.SetProgressPercentValue(100);
                    taskInformation.SetStatus(TaskStatus.Done);
                    ApplicationService.Instance.InvokeUIAction(() =>
                    {
                        RaisePropertyChanged(nameof(Tasks));
                        RaisePropertyChanged(nameof(RunningTasksCount));
                        RaisePropertyChanged(nameof(TasksInformation));
                    });
                }
                catch (Exception e)
                {
                    errAction?.Invoke(taskInformation);

                    LoggingService.Instance.Error("Occured error during running action async", e);
                    taskInformation.SetStatus(TaskStatus.Error);
                    ApplicationService.Instance.InvokeUIAction(() =>
                    {
                        RaisePropertyChanged(nameof(Tasks));
                        RaisePropertyChanged(nameof(RunningTasksCount));
                        RaisePropertyChanged(nameof(TasksInformation));
                    });
                }
            });
        }

        public void RunAsync(Action action, Action errAction = null)
        {
            RunAsync(token => action.Invoke(), token => errAction?.Invoke());
        }

        public void RunOnceAsync(string taskName, Action<ITaskReport> action, Action<ITaskReport> beginAction = null, Action<ITaskReport> endAction = null, Action<ITaskReport> errAction = null, bool doLockScreen = true, int timeSleepInMs = 0, bool canStop = true)
        {
            TaskInformation taskInformation;
            lock (_lock)
            {
                taskInformation = _tasks.FirstOrDefault(p => p.TaskName == taskName);

                if (taskInformation == null)
                {
                    taskInformation = new TaskInformation(taskName, string.Empty, action, canStop);

                    _tasks.Insert(0, taskInformation);
                }

                if (taskInformation.Status == TaskStatus.Running)
                {
                    return;
                }

                taskInformation.SetStatus(TaskStatus.Running);
                taskInformation.SetProgressPercentValue(0);
                taskInformation.CancellationToken = new CancellationTokenSource();
                RaisePropertyChanged(nameof(Tasks));
                RaisePropertyChanged(nameof(RunningTasksCount));
                RaisePropertyChanged(nameof(TasksInformation));

                if (!taskInformation.CanStop && canStop)
                {
                    LoggingService.Instance.Info($"Task {taskInformation.TaskName} was marked as not stoppable");
                }
            }

            Task.Run(() =>
            {
                LoggingService.Instance.Info($"Calling {taskName}");
                try
                {
                    if (doLockScreen)
                    {
                        ApplicationService.Instance.Busy();
                    }

                    beginAction?.Invoke(taskInformation);

                    while (!taskInformation.CancellationToken.IsCancellationRequested)
                    {
                        action.Invoke(taskInformation);
                        if (timeSleepInMs > 0)
                        {
                            if (!taskInformation.CancellationToken.IsCancellationRequested)
                            {
                                Thread.Sleep(timeSleepInMs);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    endAction?.Invoke(taskInformation);

                    LoggingService.Instance.Info($"Done! {taskName}");
                    taskInformation.SetProgressPercentValue(100);
                    taskInformation.SetStatus(TaskStatus.Done);
                    ApplicationService.Instance.InvokeUIAction(() =>
                    {
                        RaisePropertyChanged(nameof(Tasks));
                        RaisePropertyChanged(nameof(RunningTasksCount));
                        RaisePropertyChanged(nameof(TasksInformation));
                    });
                }
                catch (Exception e)
                {
                    errAction?.Invoke(taskInformation);
                    LoggingService.Instance.Error($"Error calling {taskName}", e);
                    taskInformation.SetStatus(TaskStatus.Error);
                    ApplicationService.Instance.InvokeUIAction(() =>
                    {
                        RaisePropertyChanged(nameof(Tasks));
                        RaisePropertyChanged(nameof(RunningTasksCount));
                        RaisePropertyChanged(nameof(TasksInformation));
                    });
                }
                finally
                {
                    if (doLockScreen)
                    {
                        ApplicationService.Instance.Free();
                    }
                }
            });
        }

        public void RunOnceAsync(string taskName, Action action, Action beginAction = null, Action endAction = null, Action errAction = null, bool doLockScreen = true, int timeSleepInMs = 0)
        {
            RunOnceAsync(
                taskName,
                token => action.Invoke(),
                token => beginAction?.Invoke(),
                token => endAction?.Invoke(),
                token => errAction?.Invoke(),
                doLockScreen,
                timeSleepInMs);
        }

        //public static TaskExecutionResult ExecuteCommand(string applicationPath, string command)
        //{
        //    var request = new TaskExecutionRequest
        //    {
        //        ApplicationPath = applicationPath,
        //        Command = command
        //    };

        //    return new TaskExecution().Execute(request);
        //}

        //public static TaskExecutionResult ExecuteCommand(TaskExecutionRequest request)
        //{
        //    return new TaskExecution().Execute(request);
        //}

        //public static TaskExecutionResult ExecuteCommandPrompt(string command)
        //{
        //    var request = new TaskExecutionRequest
        //    {
        //        ApplicationPath = APP_CMD,
        //        Command = $"/C {command}"
        //    };

        //    return new TaskExecution().Execute(request);
        //}

        //public static TaskExecutionResult ExecuteCommandPrompt(string command, string workingDirectory)
        //{
        //    var request = new TaskExecutionRequest
        //    {
        //        ApplicationPath = APP_CMD,
        //        Command = $"/C {command}",
        //        WorkingDirectory = workingDirectory
        //    };

        //    return new TaskExecution().Execute(request);
        //}

        //public static TaskExecutionResult ExecuteCommandPrompt(string command, string workingDirectory, TimeSpan commandTimeout)
        //{
        //    var request = new TaskExecutionRequest
        //    {
        //        ApplicationPath = APP_CMD,
        //        Command = $"/C {command}",
        //        WorkingDirectory = workingDirectory,
        //        CommandTimeout = commandTimeout
        //    };

        //    return new TaskExecution().Execute(request);
        //}

        //public TaskExecutionResult Execute(TaskExecutionRequest request)
        //{
        //    var result = new TaskExecutionResult();

        //    LoggingService.Instance.Info($"ExecuteCommand. Application: {request.ApplicationPath}. Command: {request.Command}");

        //    _process = new Process
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = request.ApplicationPath,
        //            Arguments = request.Command,
        //            RedirectStandardError = true,
        //            RedirectStandardOutput = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true,
        //            WorkingDirectory = request.WorkingDirectory
        //        }
        //    };

        //    _process.OutputDataReceived += (sender, e) =>
        //    {
        //        if (e.Data == null)
        //        {
        //            return;
        //        }

        //        result.AppendOutput(Environment.NewLine + e.Data);
        //        request.OutputDataReceived?.Invoke(e.Data, result.LastOutput);
        //    };

        //    _process.ErrorDataReceived += (sender, e) =>
        //    {
        //        if (e.Data == null)
        //        {
        //            return;
        //        }

        //        result.AppendError(Environment.NewLine + e.Data);
        //        request.ErrorDataReceived?.Invoke(e.Data, result.LastError);
        //    };

        //    _process.Start();
        //    _process.BeginOutputReadLine();
        //    _process.BeginErrorReadLine();

        //    JobService.ManageProcess(_process);

        //    var outputTimeout = request.CommandTimeout ?? TimeSpan.FromMinutes(1);
        //    var timeWatchingStart = DateTime.Now;

        //    while (!HasExist(_process))
        //    {
        //        var current = DateTime.Now;
        //        var timeGap = current - timeWatchingStart;

        //        if (timeGap.TotalMilliseconds >= outputTimeout.TotalMilliseconds)
        //        {
        //            if (request.OutputTimeoutCallback != null)
        //            {
        //                var isCancel = request.OutputTimeoutCallback.Invoke(_process, result.LastOutput);
        //                if (isCancel)
        //                {
        //                    break;
        //                }

        //                timeWatchingStart = DateTime.Now;
        //            }
        //            else
        //            {
        //                break;
        //            }
        //        }

        //        Thread.Sleep(1000);
        //    }

        //    _process.CancelOutputRead();
        //    _process.CancelErrorRead();

        //    try
        //    {
        //        if (!HasExist(_process))
        //            _process.Kill(true);
        //    }
        //    catch (Exception e)
        //    {
        //        LoggingService.Instance.Error($"Failed to kill the process", e);
        //    }

        //    _process.Close();

        //    LoggingService.Instance.Info($"ExecuteCommand. Done");

        //    return result;
        //}

        //public void Dispose()
        //{
        //    _process.CancelOutputRead();
        //    _process.CancelErrorRead();

        //    try
        //    {
        //        if (!HasExist(_process))
        //            _process.Kill(true);
        //    }
        //    catch (Exception e)
        //    {
        //        LoggingService.Instance.Error($"Failed to kill the process", e);
        //    }

        //    _process.Close();
        //    _process.Dispose();
        //}

        //private bool HasExist(Process process)
        //{
        //    var exitCode = -1;
        //    try
        //    {
        //        exitCode = process.ExitCode;
        //    }
        //    catch (Exception e) { }

        //    var hasExit = false;
        //    try
        //    {
        //        hasExit = process.HasExited;
        //    }
        //    catch (Exception e) { }

        //    return hasExit && exitCode == 0;
        //}
    }
}

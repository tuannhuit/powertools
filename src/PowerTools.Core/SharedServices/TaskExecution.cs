using Prism.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PowerTools.Core.SharedServices
{
    public class TaskExecution : BindableBase
    {
        public static readonly string APP_CMD = "C:\\Windows\\system32\\cmd.exe";
        private static readonly object _lock = new object();

        private Process _process;

        private ConcurrentDictionary<string, bool> _taskStatuses = new ConcurrentDictionary<string, bool>();

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

        }

        #endregion

        public void RunAsync(Action action, Action errAction = null)
        {
            Task.Run(() =>
            {
                try
                {
                    action.Invoke();
                }
                catch (Exception e)
                {
                    LoggingService.Instance.Error("Occured error during running action async", e);
                }
            });
        }

        public void RunOnceAsync(string taskName, Action action, Action beginAction = null, Action endAction = null, Action errAction = null)
        {
            lock (_lock)
            {
                if (!_taskStatuses.ContainsKey(taskName))
                {
                    _taskStatuses.TryAdd(taskName, false);
                }

                var isTaskRunning = _taskStatuses[taskName];
                if (isTaskRunning)
                {
                    return;
                }
                else
                {
                    _taskStatuses[taskName] = true;
                }
            }

            Task.Run(() =>
            {
                LoggingService.Instance.Info($"Calling {taskName}");
                try
                {
                    ApplicationService.Instance.Busy();

                    beginAction?.Invoke();
                    action.Invoke();
                    endAction?.Invoke();

                    LoggingService.Instance.Info($"Done! {taskName}");
                }
                catch (Exception e)
                {
                    errAction?.Invoke();
                    LoggingService.Instance.Error($"Error calling {taskName}", e);
                }
                finally
                {
                    ApplicationService.Instance.Free();

                    lock (_lock)
                    {
                        _taskStatuses[taskName] = false;
                    }
                }
            });
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

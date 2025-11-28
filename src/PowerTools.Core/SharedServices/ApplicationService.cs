using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PowerTools.Core.SharedServices
{
    public class ApplicationService
    {
        private static ApplicationService _instance;
        private static List<Action> _disposedActions = new List<Action>();

        public Action<bool> DoBusy;
        public Func<string, string, MessageBoxButton, MessageBoxResult> DoShowMessageBox;

        public static ApplicationService Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ApplicationService();

                return _instance;
            }
        }

        private ApplicationService()
        {

        }

        public void Restart()
        {
            var executionLocation = Path.GetDirectoryName(Application.ResourceAssembly.Location);
            var applicationFullPath = Path.Combine(executionLocation, "PowerTools.exe");

            Application.Current.Shutdown();
            Process.Start(applicationFullPath);
        }

        public void InvokeUIAction(Action action)
        {
            if(action == null)
            {
                return;
            }
            var d = Application.Current?.Dispatcher;

            try
            {
                if (d == null)
                {
                    return;
                }

                if (d.CheckAccess())
                {
                    action.Invoke();
                }
                else
                {
                    d.BeginInvoke(action);
                }
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error("Occured error during invoking UI action",e);
            }
        }

        public void Busy()
        {
            InvokeUIAction(() =>
            {
                DoBusy?.Invoke(true);
            });
        }

        public void Free()
        {
            InvokeUIAction(() =>
            {
                DoBusy?.Invoke(false);
            });
        }

        public MessageBoxResult MessageBox(string message, string caption = "", MessageBoxButton button = MessageBoxButton.OK)
        {
            if (DoShowMessageBox == null)
            {
                return MessageBoxResult.OK;
            }
            else
            {
                return DoShowMessageBox.Invoke(message, caption, button);
            }
        }

        public string GetOrCreateTempFolder()
        {
            string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }

        public void RegisterDisposableAction(Action disposableAction)
        {
            _disposedActions.Add(disposableAction);
        }

        public void Dispose()
        {
            foreach (var action in _disposedActions)
            {
                try
                {
                    action.Invoke();
                }
                catch(Exception e)
                {
                    LoggingService.Instance.Error("Dispose", e);
                }
            }
        }
    }
}

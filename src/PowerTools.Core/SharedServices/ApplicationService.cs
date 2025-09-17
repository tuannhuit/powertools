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
        public Action<string> DoShowMessageBox;

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

            Process.Start(applicationFullPath);
            Application.Current.Shutdown();
        }

        public void InvokeUIAction(Action action)
        {
            if(action == null)
            {
                return;
            }
            var d = Application.Current.Dispatcher;

            try
            {
                if (d.CheckAccess())
                {
                    action.Invoke();
                }
                else
                {
                    d.Invoke(action);
                }
            }
            catch (Exception e)
            {
                LoggingService.Instance.Error("Occured error during invoking UI action",e);
            }
        }

        public void Busy()
        {
            DoBusy?.Invoke(true);
        }

        public void Free()
        {
            DoBusy?.Invoke(false);
        }

        public void MessageBox(string message)
        {
            DoShowMessageBox?.Invoke(message);
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

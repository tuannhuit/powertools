using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace PowerTools.Core.SharedServices
{
    public class ApplicationService
    {
        private static ApplicationService _instance;
        private static List<Action> _disposedActions = new List<Action>();

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

        public void RegisterDisposableAction(Action disposableAction)
        {
            _disposedActions.Add(disposableAction);
        }

        public void Dispose()
        {
            foreach (var action in _disposedActions)
            {
                action.Invoke();
            }
        }
    }
}

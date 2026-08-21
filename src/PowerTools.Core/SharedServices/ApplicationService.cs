using PowerTools.Core.Models;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PowerTools.Core.SharedServices
{
    public class ApplicationService : BindableBase
    {
        private static ApplicationService _instance;
        private static List<Action> _disposedActions = new List<Action>();
        public IDialogService DialogService { get; set; }
        public IContainerExtension ContainerExtension { get; set; }
        public Window MainWindow { get; set; }
        public ContentControl DialogView { get; set; }
        //public Action<bool> DoBusy;
        public bool IsFree => !_isBusy;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsFree");
            }
        }

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

        private string _waitMessage;
        /// <summary>
        /// The wait message showing in Waiting Window Screen
        /// </summary>
        public string WaitMessage
        {
            get => _waitMessage;
            set
            {
                _waitMessage = value;
                RaisePropertyChanged();
            }
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
            if (action == null)
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
                LoggingService.Instance.Error("Occured error during invoking UI action", e);
            }
        }

        public void Busy()
        {
            Busy("Please wait...");
        }

        public void Busy(string waitMessage)
        {
            InvokeUIAction(() =>
            {
                WaitMessage = waitMessage;
                IsBusy=true;
                //DoBusy?.Invoke(true);
            });
        }

        public void Free()
        {
            InvokeUIAction(() =>
            {
                IsBusy=false;
                //DoBusy?.Invoke(false);
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

        public void ShowDialog<TView>(string title, object viewModel, Action<IDialogResult> callback, IList<DialogAction> actions = null, double width = 0, double height = 0)
        {
            var viewType = typeof(TView);
            var viewName = viewType.Name;
            if (!ContainerExtension.IsRegistered(viewType, viewName))
            {
                ContainerExtension.RegisterForNavigation(viewType, viewName);
            }

            var parameters = new DialogInformation
            {
                Title = title,
                ViewType = viewType,
                Actions = actions ?? new List<DialogAction>(),
                Callback = callback,
                ViewModel = viewModel,
                Width = width,
                Height = height
            };

            var dialogParameters = new DialogParameters
            {
                {"dialogInformation", parameters}
            };

            DialogService.ShowDialog("DialogView", dialogParameters, parameters.Callback);
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
                catch (Exception e)
                {
                    LoggingService.Instance.Error("Dispose", e);
                }
            }
        }
    }
}

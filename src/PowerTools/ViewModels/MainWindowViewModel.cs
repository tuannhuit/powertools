using System;
using System.Linq;
using PowerTools.Core.Configurations;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.ViewModels.UserControls;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System.Windows;
using System.Windows.Input;

namespace PowerTools.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IContainerProvider _container;
        private IDialogService _dialogService;

        private GridLength _viewLogGridLength;
        public GridLength ViewLogGridLength
        {
            get => _viewLogGridLength;
            set
            {
                _viewLogGridLength = value;
                LoggingService.Instance.DoShowLog = ViewLogGridLength.Value >= 10;
                RaisePropertyChanged();
            }
        }
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

        #region Commands
        public ICommand CmdShowSettings { get; set; }
        public ICommand CmdShowLog { get; set; }
        public ICommand CmdSelectModuleList { get; set; }
        public ICommand CmdSelectModuleKafka { get; set; }
        public ICommand CmdSelectModuleQAutomation { get; set; }

        #endregion

        public MainWindowViewModel(IContainerProvider container, IRegionManager regionManager, IDialogService dialogService)
        {
            _container = container;
            _dialogService = dialogService;

            LoggingService.Instance.DoShowLogCallback = DoShowLogCallback;
            ApplicationService.Instance.DoBusy = DoBusy;
            ApplicationService.Instance.DoShowMessageBox = DoShowMessageBox;

            ViewLogGridLength = new GridLength(0);

            CmdShowLog = new DelegateCommand(OnCmdShowLog);
            CmdShowSettings = new DelegateCommand(OnCmdShowSettings);
            CmdSelectModuleList = new DelegateCommand(OnCmdSelectModuleList);
            CmdSelectModuleKafka = new DelegateCommand(OnCmdSelectModuleKafka);
            CmdSelectModuleQAutomation = new DelegateCommand(OnCmdSelectModuleQAutomation);

            RepositoryLoader.Instance.LoadLocalRepository();


            foreach (var toolModule in RepositoryLoader.Instance.LocalRepository.ModuleList)
            {
                try
                {
                    ModuleLoader.Instance.LoadModule(_container, toolModule);
                }
                catch (Exception e)
                {

                }
            }

            ViewNavigator.Instance.NavigateToModuleLoaderView(_container);

            DownloadPowerToolVersions();
            _dialogService = dialogService;
        }

        private void DownloadPowerToolVersions()
        {

        }

        private MessageBoxResult DoShowMessageBox(string message, string caption, MessageBoxButton button)
        {
            if (Application.Current?.MainWindow != null)
            {
                return MessageBox.Show(Application.Current.MainWindow, message, caption, button);
            }
            else
            {
                return MessageBox.Show(message, caption, button);
            }
        }

        private void DoBusy(bool doBusy)
        {
            IsBusy = doBusy;
        }

        private void DoShowLogCallback(bool doShowLogs)
        {
            if (doShowLogs)
            {
                if (ViewLogGridLength.Value < 100)
                {
                    LoggingService.Instance.DoShowLog = true;
                    ViewLogGridLength = new GridLength(100);
                }
            }
            else
            {
                ViewLogGridLength = new GridLength(0);
                LoggingService.Instance.DoShowLog = false;
            }
        }

        private void OnCmdShowLog()
        {
            if (ViewLogGridLength.Value > 0)
            {
                LoggingService.Instance.DoShowLog = false;
                ViewLogGridLength = new GridLength(0);
            }
            else
            {
                LoggingService.Instance.DoShowLog = true;
                ViewLogGridLength = new GridLength(100);
            }
        }

        private void OnCmdShowSettings()
        {
            var settings = ModuleGlobalSettings.Instance.LoadApplicationConfigurationsAsList();
            var dialogParams = new DialogParameters
            {
                { "settings", settings }
            };

            _dialogService.ShowDialog("ModuleSettingsView", dialogParams, callback =>
            {
                if (callback.Result == ButtonResult.OK)
                {
                    var result = callback.Parameters.GetValue<ModuleSettingsViewModel>("ModuleSettingsViewModel");
                    ModuleGlobalSettings.Instance.SaveModuleConfigurations(result.GetModuleSettingsAsDictionary(), true);
                }
            });
        }

        private void OnCmdSelectModuleList()
        {
            ViewNavigator.Instance.NavigateToModuleLoaderView(_container);
        }

        private void OnCmdSelectModuleKafka()
        {
            var module = RepositoryLoader.Instance.LocalRepository.ModuleList.First(p => p.Name == "Kafka Client Tool");

            ViewNavigator.Instance.NavigateToModuleView(_container, module);
        }

        private void OnCmdSelectModuleQAutomation()
        {
            var module = RepositoryLoader.Instance.LocalRepository.ModuleList.First(p => p.Name == "QAutomation Tool");

            ViewNavigator.Instance.NavigateToModuleView(_container, module);
        }
    }
}

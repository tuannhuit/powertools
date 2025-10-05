using PowerTools.Core.Configurations;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Windows;
using System.Windows.Input;

namespace PowerTools.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IContainerProvider _container;
        private readonly IRegionManager _regionManager;

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

        public ICommand CmdShowLog { get; set; }

        public MainWindowViewModel(IContainerProvider container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;

            LoggingService.Instance.DoShowLogCallback = DoShowLogCallback;
            ApplicationService.Instance.DoBusy = DoBusy;
            ApplicationService.Instance.DoShowMessageBox = DoShowMessageBox;

            ViewLogGridLength = new GridLength(0);

            CmdShowLog = new DelegateCommand(OnCmdShowLog);

            RepositoryLoader.Instance.LoadLocalRepository();

            if (RepositoryLoader.Instance.LocalRepository.SelectedModule != null)
            {
                ModuleGlobalSettings.Instance.CurrentModule = RepositoryLoader.Instance.LocalRepository.SelectedModule;
                ModuleLoader.Instance.LoadModule(_container, ModuleGlobalSettings.Instance.CurrentModule);
                ViewNavigator.Instance.NavigateToModuleView(_container);
            }
            else
            {
                ViewNavigator.Instance.NavigateToModuleLoaderView(_container);
            }

            DownloadPowerToolVersions();
        }

        private void DownloadPowerToolVersions()
        {

        }

        private void DoShowMessageBox(string message)
        {
            MessageBox.Show(Application.Current.MainWindow, message);
        }

        private void DoBusy(bool doBusy)
        {
            IsBusy = doBusy;
        }

        private void DoShowLogCallback(bool doShowLogs)
        {
            if (doShowLogs)
            {
                if (ViewLogGridLength.Value < 25)
                {
                    LoggingService.Instance.DoShowLog = true;
                    ViewLogGridLength = new GridLength(25);
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
                ViewLogGridLength = new GridLength(25);
            }
        }
    }
}

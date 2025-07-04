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
                RaisePropertyChanged();
            }
        }
        public ICommand CmdShowLog { get; set; }

        public MainWindowViewModel(IContainerProvider container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;

            LoggingService.Instance.DoShowLogCallback = DoShowLogCallback;
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
        }

        private void DoShowLogCallback(bool doShowLogs)
        {
            if (doShowLogs)
            {
                if(ViewLogGridLength.Value < 50)
                {
                    ViewLogGridLength = new GridLength(100);
                }
            }
            else
            {
                ViewLogGridLength = new GridLength(0);
            }
        }

        private void OnCmdShowLog()
        {
            if (ViewLogGridLength.Value > 0)
            {
                ViewLogGridLength = new GridLength(0);
            }
            else
            {
                ViewLogGridLength = new GridLength(100);
            }
        }
    }
}

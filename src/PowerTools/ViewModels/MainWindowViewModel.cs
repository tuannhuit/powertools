using System;
using System.Collections.ObjectModel;
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
using PowerTools.Core.Models;

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

        private bool _isActiveModuleList;
        public bool IsActiveModuleList
        {
            get => _isActiveModuleList;
            set
            {
                _isActiveModuleList = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<ToolModule> ModuleList => new(Repositories.RepositoryLocal.ModuleList.Where(p => p.IsInstalled));

        #region Commands
        public ICommand CmdShowSettings { get; set; }
        public ICommand CmdShowLog { get; set; }
        public ICommand CmdSelectModuleList { get; set; }
        public ICommand CmdNavigateToModule { get; set; }
        public ICommand CmdClearLogs { get; set; }

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
            CmdNavigateToModule = new DelegateCommand<string>(OnCmdNavigateToModule);
            CmdClearLogs = new DelegateCommand(OnCmdClearLogs);

            Repositories.RepositoryLocal.Load(false);
            ModuleLoader.Container = _container;
            foreach (var toolModule in Repositories.RepositoryLocal.ModuleList)
            {
                if (toolModule.IsActive)
                {
                    try
                    {
                        ModuleLoader.LoadModule(toolModule);
                        toolModule.IsLoadedProperly = true;
                    }
                    catch (Exception e)
                    {
                        toolModule.IsLoadedProperly = false;
                        toolModule.IsActive = false;
                        LoggingService.Instance.Error($"Failed to load module '{toolModule.Name}'", e);
                    }
                }
                else
                {
                    toolModule.IsLoadedProperly = true;
                }
            }

            var activeModule = Repositories.RepositoryLocal.ModuleList.FirstOrDefault(p => p.IsSelected);
            if (activeModule != null)
            {
                ModuleGlobalSettings.Instance.CurrentModule = activeModule;
                OnCmdNavigateToModule(activeModule.Name);
            }
            else
            {
                OnCmdSelectModuleList();
            }

            Repositories.RepositoriesChanged += Repositories_RepositoriesChanged;

            DownloadPowerToolVersions();
        }

        private void OnCmdClearLogs()
        {
            LoggingService.Instance.Clear();
        }

        private void Repositories_RepositoriesChanged()
        {
            RaisePropertyChanged("ModuleList");
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
            IsActiveModuleList = true;
            ViewNavigator.Instance.NavigateToModuleLoaderView(_container);
            Repositories.RepositoryLocal.ModuleList.ForEach(p => p.IsSelected = false);
        }

        private void OnCmdNavigateToModule(string moduleName)
        {
            IsActiveModuleList = false;
            var module = Repositories.RepositoryLocal.ModuleList.First(p => p.Name == moduleName);
            if (module != null)
            {
                try
                {
                    ViewNavigator.Instance.NavigateToModuleView(_container, module);
                }
                catch (Exception e)
                {
                    LoggingService.Instance.Error($"Error to load module {moduleName}", e);
                }
            }
        }
    }
}

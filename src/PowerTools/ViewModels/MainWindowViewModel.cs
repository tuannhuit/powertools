using Octokit;
using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using PowerTools.Utils;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using PowerTools.Models;
using Application = System.Windows.Application;
using ModuleSettings = PowerTools.Views.UserControls.ModuleSettings;

namespace PowerTools.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private IDialogService _dialogService;
        private AppVersion _appVersion;

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

        public AppVersion AppVersion
        {
            get => _appVersion;
            set => SetProperty(ref _appVersion, value);
        }

        public ObservableCollection<ToolModule> ModuleList => new(Repositories.RepositoryLocal.ModuleList.Where(p => p.IsInstalled));

        #region Commands
        public ICommand CmdShowSettings { get; set; }
        public ICommand CmdShowLog { get; set; }
        public ICommand CmdSelectModuleList { get; set; }
        public ICommand CmdNavigateToModule { get; set; }
        public ICommand CmdClearLogs { get; set; }

        #endregion

        public MainWindowViewModel(IContainerProvider container, IRegionManager regionManager, IDialogService dialogService, IContainerExtension containerExtension)
        {
            //var localFile = @"C:\Temp\test.json";
            //using (var webClient = new WebClient())
            //{
            //    webClient.DownloadFile("file://hsnicx-fg01/icxteamcitybucket/Teams/Delta/Tools/PowerTool/modules/repository-v3.0.0.0-PREVIEW.json", localFile);
            //}

            _dialogService = dialogService;

            LoggingService.Instance.DoShowLogCallback = DoShowLogCallback;
            //ApplicationService.Instance.DoBusy = DoBusy;
            ApplicationService.Instance.DoShowMessageBox = DoShowMessageBox;
            ApplicationService.Instance.DialogService = dialogService;
            ApplicationService.Instance.ContainerExtension = containerExtension;

            ViewLogGridLength = new GridLength(0);

            CmdShowLog = new DelegateCommand(OnCmdShowLog);
            CmdShowSettings = new DelegateCommand(OnCmdShowSettings);
            CmdSelectModuleList = new DelegateCommand(OnCmdSelectModuleList);
            CmdNavigateToModule = new DelegateCommand<string>(OnCmdNavigateToModule);
            CmdClearLogs = new DelegateCommand(OnCmdClearLogs);

            Repositories.RepositoryLocal.Load(false);
            ModuleLoader.Container = containerExtension;
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
            
            AppVersion = new AppVersion();
            AppVersion.CheckForUpdate();

            ApplicationService.Instance.RegisterDisposableAction(() =>
            {
                AppVersion.InstallNewVersion();
            });
        }


        private void OnCmdClearLogs()
        {
            LoggingService.Instance.Clear();
        }

        private void Repositories_RepositoriesChanged()
        {
            RaisePropertyChanged("ModuleList");
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

        //private void DoBusy(bool doBusy)
        //{
        //    IsBusy = doBusy;
        //}

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

            var moduleSettings = new Models.ModuleSettings();
            moduleSettings.SetSettings(settings);

            var actions = new List<DialogAction>
            {
                new DialogAction
                {
                    Icon = ButtonIcons.Revert,
                    Name = "Revert",
                    Action = (actionParams) =>
                    {
                        moduleSettings.RevertSettings();
                    }
                },
                new DialogAction
                {
                    Icon = ButtonIcons.Save,
                    Name = "Save & Close",
                    Action = (actionParams) =>
                    {
                        ModuleGlobalSettings.Instance.SaveModuleConfigurations(moduleSettings.GetModuleSettingsAsDictionary(), true);
                    },
                    DoCloseWhenInvoked = true
                },
            };

            ApplicationService.Instance.ShowDialog<ModuleSettings>(
                "Module Settings | Edit",
                moduleSettings,
                null,
                actions,
                850,
                480);
        }

        private void OnCmdSelectModuleList()
        {
            IsActiveModuleList = true;
            ViewNavigator.Instance.NavigateToModuleLoaderView();
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
                    ViewNavigator.Instance.NavigateToModuleView(module);
                }
                catch (Exception e)
                {
                    LoggingService.Instance.Error($"Error to load module {moduleName}", e);
                }
            }
        }
    }
}

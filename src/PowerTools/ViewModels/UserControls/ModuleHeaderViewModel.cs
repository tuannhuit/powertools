using Microsoft.Xaml.Behaviors.Core;
using PowerTools.Core.Configurations;
using PowerTools.Core.SharedServices;
using PowerTools.Helpers;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Windows.Input;

namespace PowerTools.ViewModels.UserControls
{
    public class ModuleHeaderViewModel : BindableBase
    {
        private readonly IContainerProvider _container;
        public ICommand CmdOpenModuleSettings { get; set; }
        public ICommand CmdMoveBack { get; set; }

        private IDialogService _dialogService;

        public ModuleHeaderViewModel(IContainerProvider container, IDialogService dialogService)
        {
            this._container = container;
            this._dialogService = dialogService;

            CmdOpenModuleSettings = new ActionCommand(OnCmdOpenModuleSettings);
            CmdMoveBack = new ActionCommand(OnCmdMoveBack);
        }

        private void OnCmdMoveBack()
        {
            ViewNavigator.Instance.NavigateToModuleLoaderView(_container);
            ApplicationService.Instance.Dispose();
            ModuleGlobalSettings.Instance.CurrentModule = null;
            RepositoryLoader.Instance.Store();
            LoggingService.Instance.Info("Ready");
        }

        private void OnCmdOpenModuleSettings()
        {
            var settings = ModuleGlobalSettings.Instance.LoadModuleConfigurationsAsList();
            var dialogParams = new DialogParameters();
            dialogParams.Add("settings", settings);

            _dialogService.ShowDialog("ModuleSettingsView", dialogParams, callback =>
            {
                if (callback.Result == ButtonResult.OK)
                {
                    var result = callback.Parameters.GetValue<ModuleSettingsViewModel>("ModuleSettingsViewModel");
                    ModuleGlobalSettings.Instance.SaveModuleConfigurations(result.GetModuleSettingsAsDictionary(), true);
                }
            });
        }
    }
}

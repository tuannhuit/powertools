using PowerTools.Core.Configurations;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Windows.Input;

namespace PowerTools.ViewModels.UserControls
{
    public class ModuleSettingsViewModel : BindableBase, IDialogAware
    {
        public string Title => "Module Settings";
        public event Action<IDialogResult> RequestClose;

        private string _moduleSettings;
        public string ModuleSettings
        {
            get => _moduleSettings;
            set
            {
                _moduleSettings = value;
                RaisePropertyChanged();
                RaisePropertyChanged("ModuleSettingsValid");
            }
        }

        public bool ModuleSettingsValid => !string.IsNullOrEmpty(ModuleSettings);

        public ICommand CmdSaveSettings { get; set; }

        public ModuleSettingsViewModel()
        {
            CmdSaveSettings = new DelegateCommand(OnCmdSaveSettings).ObservesCanExecute(() => ModuleSettingsValid);
        }

        private void OnCmdSaveSettings()
        {
            var result = ButtonResult.OK;
            var param = new DialogParameters
            {
                { "ModuleSettingsViewModel", this }
            };
            RequestClose?.Invoke(new DialogResult(result, param));
        }

        public bool CanCloseDialog() => true;

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            ModuleSettings = ModuleGlobalSettings.Instance.LoadModuleConfigurationsAsString();
        }
    }
}

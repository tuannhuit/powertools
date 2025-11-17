using PowerTools.Core.Configurations;
using PowerTools.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PowerTools.ViewModels.UserControls
{
    public class ModuleSettingsViewModel : BindableBase, IDialogAware
    {
        public string Title => "Module Settings";
        public event Action<IDialogResult> RequestClose;

        private ObservableCollection<SettingItem> _moduleSettings;
        public ObservableCollection<SettingItem> ModuleSettings
        {
            get => _moduleSettings;
            set
            {
                _moduleSettings = value;
                RaisePropertyChanged();
                RaisePropertyChanged("ModuleSettingsValid");
            }
        }

        public bool ModuleSettingsValid => OnValidateSettingsChanged();

        public ICommand CmdSaveSettings { get; set; }
        public ICommand CmdRevertSettings { get; set; }

        public ModuleSettingsViewModel()
        {
            ModuleSettings = new ObservableCollection<SettingItem>();
            CmdSaveSettings = new DelegateCommand(OnCmdSaveSettings).ObservesCanExecute(() => ModuleSettingsValid);
            CmdRevertSettings = new DelegateCommand(OnCmdRevertSettings).ObservesCanExecute(() => ModuleSettingsValid);
        }

        private void OnCmdRevertSettings()
        {
            foreach (var moduleSetting in ModuleSettings)
            {
                moduleSetting.Value = moduleSetting.OriginalValue;
            }
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
            var settings = parameters.GetValue<List<KeyValuePair<string, string?>>>("settings");
            foreach (var item in settings)
            {
                var newSettingItem = new SettingItem(item, item.Value, OnValueChanged);
                ModuleSettings.Add(newSettingItem);
            }
        }

        public Dictionary<string, string> GetModuleSettingsAsDictionary()
        {
            var settings = new Dictionary<string, string>();
            foreach (var item in ModuleSettings)
            {
                settings.Add(item.Key, item.Value);
            }

            return settings;
        }

        private void OnValueChanged(string filterName, object valueChanged)
        {
            RaisePropertyChanged("ModuleSettingsValid");
        }

        private bool OnValidateSettingsChanged()
        {
            if (ModuleSettings == null || !ModuleSettings.Any())
            {
                return false;
            }

            return ModuleSettings.Any(p => p.IsValueChanged);
        }
    }
}

using PowerTools.Core.Models;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace PowerTools.Models
{
    public class ModuleSettings : BindableBase
    {
        private PaginationCollection<SettingItem> _settings;
        private SettingItem _selectedSetting;

        public PaginationCollection<SettingItem> Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }
        public SettingItem SelectedSetting
        {
            get => _selectedSetting;
            set => SetProperty(ref _selectedSetting, value);
        }


        public void SetSettings(List<KeyValuePair<string, string>> settings)
        {
            Settings = new PaginationCollection<SettingItem>();

            foreach (var item in settings)
            {
                var newSettingItem = new SettingItem(item, item.Value);
                Settings.AddItem(newSettingItem);
            }

            SelectedSetting = _settings.Items.FirstOrDefault();
        }

        public void RevertSettings()
        {
            foreach (var moduleSetting in Settings.ItemSource)
            {
                moduleSetting.Value = moduleSetting.OriginalValue;
            }
        }

        public Dictionary<string, string> GetModuleSettingsAsDictionary()
        {
            var settings = new Dictionary<string, string>();
            foreach (var item in Settings.ItemSource)
            {
                settings.Add(item.Key, item.Value);
            }

            return settings;
        }
    }
}

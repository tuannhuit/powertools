using PowerTools.Core.Models;
using Prism.Mvvm;
using System.Collections.Generic;

namespace PowerTools.Models
{
    public class SettingItem : BindableBase
    {
        private event ModelFilterHandler _handler;

        private string _key;
        public string Key
        {
            get => _key;
            set
            {
                _key = value;
                RaisePropertyChanged();
            }
        }

        private string? _value;
        public string? Value
        {
            get => _value;
            set
            {
                _value = value;
                RaisePropertyChanged();
                IsValueChanged = _value != OriginalValue;

                _handler?.Invoke(Key, _value);
            }
        }

        public string? OriginalValue
        {
            get;
            private set;
        }

        private bool _isValueChanged;
        public bool IsValueChanged
        {
            get => _isValueChanged;
            private set
            {
                _isValueChanged = value;
                RaisePropertyChanged();
            }
        }

        public SettingItem()
        {

        }

        public SettingItem(string key, string? value, string? originalValue, ModelFilterHandler handler = null)
        {
            OriginalValue = originalValue;
            _handler = handler;

            Key = key;
            Value = value;
        }

        public SettingItem(KeyValuePair<string, string?> item, string? originalValue, ModelFilterHandler handler = null)
        {
            OriginalValue = originalValue;
            _handler = handler;

            Key = item.Key;
            Value = item.Value;
        }
    }
}
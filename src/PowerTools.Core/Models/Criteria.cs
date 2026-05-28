using Prism.Mvvm;
using System.Runtime.CompilerServices;

namespace PowerTools.Core.Models
{
    public delegate void CriteriaHandler(string criteria, object valueChanged);

    public class Criteria : BindableBase
    {
        protected CriteriaHandler PreValueChangedHandler;
        public CriteriaHandler ValueChangedHandler { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnRaisePropertyChanged(value);
            }
        }

        private string _description;
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnRaisePropertyChanged(value);
            }
        }

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                _value = value;
                OnRaisePropertyChanged(value);
            }
        }

        public Criteria(string name, string description, object value, CriteriaHandler valueChangedHandler)
        {
            Name = name;
            Description = description;
            Value = value;
            ValueChangedHandler = valueChangedHandler;
        }

        public Criteria(string name, string description, object value)
        {
            Name = name;
            Description = description;
            Value = value;
        }

        public Criteria(string name, string description, CriteriaHandler valueChangedHandler)
        {
            Name = name;
            Description = description;
            ValueChangedHandler = valueChangedHandler;
        }

        public Criteria(string name, CriteriaHandler valueChangedHandler)
            : this(name, name, valueChangedHandler)
        {

        }

        public Criteria(string name)
            : this(name, name, null)
        {

        }

        public void SetValueChangedHandler(CriteriaHandler valueChangedHandler)
        {
            PreValueChangedHandler = valueChangedHandler;
        }

        protected void OnRaisePropertyChanged<T>(T valueChanged = default(T), [CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(propertyName);
            PreValueChangedHandler?.Invoke(propertyName, valueChanged);
            ValueChangedHandler?.Invoke(propertyName, valueChanged);
        }
    }
}

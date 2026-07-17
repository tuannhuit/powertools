using Prism.Mvvm;
using System.Runtime.CompilerServices;

namespace PowerTools.Core.Models
{
    public enum ValueChangedEvent
    {
        PropertyChanged,
        KeyEnter
    }

    public class ValueChangedParameter
    {
        public string Criteria { get; set; }
        public ValueChangedEvent Reason { get; set; }
        public object ValueChanged { get; set; }
    }

    public delegate void ValueChangedHandler(ValueChangedParameter arg);

    public class Criteria : BindableBase
    {
        /// <summary>
        /// The ValueChanged callback is called before ValueChangedHandler
        /// </summary>
        public ValueChangedHandler ValueCallbackHandler { get; set; }

        /// <summary>
        /// Data transformation is called to make up data before ValueChangedHandler
        /// </summary>
        public event ValueChangedHandler ValueChanged;
        public ValueChangedHandler ValueChangedHandler { get; set; }

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

        public Criteria(string name, string description, object value, ValueChangedHandler valueChangedHandler)
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

        public Criteria(string name, string description, ValueChangedHandler valueChangedHandler)
        {
            Name = name;
            Description = description;
            ValueChangedHandler = valueChangedHandler;
        }

        public Criteria(string name, ValueChangedHandler valueChangedHandler)
            : this(name, name, valueChangedHandler)
        {

        }

        public Criteria(string name)
            : this(name, name, null)
        {

        }

        protected void OnRaisePropertyChanged(object valueChanged, [CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(propertyName);

            var valueChangedParameter = new ValueChangedParameter
            {
                Criteria = propertyName,
                ValueChanged = valueChanged,
                Reason =  ValueChangedEvent.PropertyChanged
            };

            ValueCallbackHandler?.Invoke(valueChangedParameter);

            if (ValueChanged != null)
            {
                ValueChanged.Invoke(valueChangedParameter);
            }

            ValueChangedHandler?.Invoke(valueChangedParameter);
        }

        public virtual void ClearValue(bool doNotify)
        {
            if (doNotify)
            {
                Value = null;
            }
            else
            {
                _value = null;
                RaisePropertyChanged(nameof(Value));
            }
        }
    }
}

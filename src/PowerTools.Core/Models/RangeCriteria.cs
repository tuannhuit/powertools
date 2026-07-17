namespace PowerTools.Core.Models
{
    public class RangeCriteria : Criteria
    {
        private object _value2;
        public object Value2
        {
            get => _value2;
            set
            {
                _value2 = value;
                OnRaisePropertyChanged(value);
            }
        }

        private string _description2;
        public string Description2
        {
            get => _description2;
            set
            {
                _description2 = value;
                OnRaisePropertyChanged(value);
            }
        }

        public RangeCriteria(string name, string description, string description2, object value, object value2, ValueChangedHandler valueChangedHandler) : base(name, description, value, valueChangedHandler)
        {
            Description2 = description2;
            Value2 = value2;
        }

        public RangeCriteria(string name, string description, string description2, object value, object value2) : base(name, description, value)
        {
            Description2 = description2;
            Value2 = value2;
        }

        public RangeCriteria(string name, string description, string description2, ValueChangedHandler valueChangedHandler) : base(name, description, valueChangedHandler)
        {
            Description2 = description2;
        }

        public RangeCriteria(string name, ValueChangedHandler valueChangedHandler)
            : base(name, name, valueChangedHandler)
        {

        }

        public RangeCriteria(string name)
            : base(name, name, null)
        {

        }

        public override void ClearValue(bool doNotify)
        {
            base.ClearValue(doNotify);

            if (doNotify)
            {
                Value2 = null;
            }
            else
            {
                _value2 = null;
                RaisePropertyChanged(nameof(Value2));
            }
        }
    }
}

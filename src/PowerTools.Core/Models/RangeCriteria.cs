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

        public RangeCriteria(string name, string description, string description2, object value, object value2, CriteriaHandler valueChangedHandler) : base(name, description, value, valueChangedHandler)
        {
            Description2 = description2;
            Value2 = value2;
        }

        public RangeCriteria(string name, string description, string description2, CriteriaHandler valueChangedHandler) : base(name, description, valueChangedHandler)
        {
            Description2 = description2;
        }

        public RangeCriteria(string name, CriteriaHandler valueChangedHandler)
            : base(name, name, valueChangedHandler)
        {

        }

        public void SetValue2(object value)
        {
            _value2 = value;
            RaisePropertyChanged("Value2");
            PreValueChangedHandler?.Invoke("Value2", value);
        }
    }
}

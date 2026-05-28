namespace PowerTools.Core.Models
{
    public class MultipleValueCriteria: Criteria
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

        public MultipleValueCriteria(string name, string description, ValueChangedHandler valueChangedHandler)
            : base(name, description, CriteriaType.Range, valueChangedHandler)
        {

        }

        public MultipleValueCriteria(string name, ValueChangedHandler valueChangedHandler)
            : base(name, name, CriteriaType.Range, valueChangedHandler)
        {

        }
    }
}

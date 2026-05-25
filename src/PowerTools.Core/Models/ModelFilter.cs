using Prism.Mvvm;
using System.Runtime.CompilerServices;

namespace PowerTools.Core.Models
{
    public delegate void ModelFilterHandler(string filterName, object valueChanged);

    public class ModelFilter: BindableBase
    {
        protected ModelFilterHandler Handler { get; set; }
        public ModelFilter(ModelFilterHandler handler)
        {
            Handler = handler;
            RaisePropertyChanged();
        }

        protected void OnRaisePropertyChanged<T>(T valueChanged = default(T), [CallerMemberName] string propertyName = null)
        {
            RaisePropertyChanged(propertyName);
            this.Handler?.Invoke(propertyName, valueChanged);
        }
    }
}

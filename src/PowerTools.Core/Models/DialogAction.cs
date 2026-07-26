using Prism.Mvvm;
using System;
using System.Linq.Expressions;

namespace PowerTools.Core.Models
{
    public class DialogAction : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        private string _icon;
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                RaisePropertyChanged();
            }
        }

        private bool _doCloseWhenInvoked;
        public bool DoCloseWhenInvoked
        {
            get => _doCloseWhenInvoked;
            set
            {
                _doCloseWhenInvoked = value;
                RaisePropertyChanged();
            }
        }

        public Action<ActionParams> Action { get; set; }
        public Expression<Func<bool>> CanExecuteExpression { get; set; }
    }
}

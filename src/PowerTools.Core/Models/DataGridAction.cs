using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Mvvm;

namespace PowerTools.Core.Models
{
    public class DataGridAction : BindableBase
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

        private ICommand _command;
        public ICommand Command
        {
            get => _command;
            set
            {
                _command = value;
                RaisePropertyChanged();
            }
        }
    }
}

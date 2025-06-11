using PowerTools.Core.Configurations;
using Prism.Modularity;
using Prism.Mvvm;

namespace PowerTools.ViewModels
{
    public class ModuleWindowViewModel : BindableBase
    {
        private string _regionModuleView;

        public string RegionModuleView
        {
            get => _regionModuleView;
            set
            {
                _regionModuleView = value;
                RaisePropertyChanged("RegionModuleView");
            }
        }

        public ModuleWindowViewModel(IModuleManager moduleManager)
        {

            moduleManager.LoadModule(ModuleGlobalSettings.Instance.CurrentModule.Name);
        }
    }
}

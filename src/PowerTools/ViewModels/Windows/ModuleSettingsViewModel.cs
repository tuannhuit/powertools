using PowerTools.Core.Configurations;
using Prism.Mvvm;

namespace PowerTools.ViewModels.Windows
{
    public class ModuleSettingsViewModel : BindableBase
    {

        public string RepositoryPath
        {
            get => ModuleGlobalSettings.Instance.RepositoryRemote;
            set
            {
                ModuleGlobalSettings.Instance.RepositoryRemote = value;
                RaisePropertyChanged(RepositoryPath);
            }
        }
    }
}

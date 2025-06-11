using PowerTools.Core.Configurations;
using PowerTools.Helpers;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;

namespace PowerTools.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IContainerProvider _container;
        private readonly IRegionManager _regionManager;

        public MainWindowViewModel(IContainerProvider container, IRegionManager regionManager)
        {
            _container = container;
            _regionManager = regionManager;

            RepositoryLoader.Instance.LoadLocalRepository();

            if (RepositoryLoader.Instance.LocalRepository.SelectedModule != null)
            {
                ModuleGlobalSettings.Instance.CurrentModule = RepositoryLoader.Instance.LocalRepository.SelectedModule;
                ModuleLoader.Instance.LoadModule(_container, ModuleGlobalSettings.Instance.CurrentModule);
                ViewNavigator.Instance.NavigateToModuleView(_container);
            }
            else
            {
                ViewNavigator.Instance.NavigateToModuleLoaderView(_container);
            }
        }
    }
}

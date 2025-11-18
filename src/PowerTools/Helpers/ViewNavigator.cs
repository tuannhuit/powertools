using PowerTools.Core.Configurations;
using PowerTools.Views;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Linq;
using PowerTools.Core.Models;

namespace PowerTools.Helpers
{
    public class ViewNavigator
    {
        private static ViewNavigator _instance;

        private ViewNavigator() { }

        public static ViewNavigator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ViewNavigator();
                }

                return _instance;
            }
        }

        public void NavigateToModuleView(IContainerProvider container,  ToolModule module)
        {
            var region = container.Resolve<IRegionManager>();
            if (region == null) return;

            ModuleGlobalSettings.Instance.CurrentModule = module;

            var viewType = ModuleViewSelectionHelper.GetView(module.Name);
            if (!IsExistedNavigation(region, Constants.ModuleRegionName, viewType))
            {
                region.RegisterViewWithRegion(Constants.ModuleRegionName, viewType);
            }

            region.RequestNavigate(Constants.ModuleRegionName, viewType.FullName);
        }

        public void NavigateToModuleLoaderView(IContainerProvider container)
        {
            var region = container.Resolve<IRegionManager>();
            if (region == null) return;

            if (!IsExistedNavigation(region, Constants.ModuleRegionName, typeof(ModuleList)))
            {
                region.RegisterViewWithRegion(Constants.ModuleRegionName, typeof(ModuleList));
            }

            region.RequestNavigate(Constants.ModuleRegionName, new Uri("ModuleList", UriKind.Relative));
        }

        private bool IsExistedNavigation(IRegionManager regionManager, string regionName, Type viewType)
        {
            var selectedRegion = regionManager.Regions.FirstOrDefault(p => p.Name == regionName);
            if (selectedRegion == null) return false;

            var selectedView = selectedRegion.Views.FirstOrDefault(p => p.GetType().FullName == viewType.FullName);
            if (selectedView == null) return false;

            return true;
        }
    }
}

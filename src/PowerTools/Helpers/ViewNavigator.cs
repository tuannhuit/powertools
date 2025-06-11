using PowerTools.Core.Configurations;
using PowerTools.Views;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Linq;

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

        public void NavigateToModuleView(IContainerProvider container)
        {
            var region = container.Resolve<IRegionManager>();
            if (region == null) return;

            if (!IsExistedNaviation(region,  Constants.MasterRegionName, typeof(ModuleWindow)))
            {
                region.RegisterViewWithRegion(Constants.MasterRegionName, typeof(ModuleWindow));
            }

            region.RequestNavigate(Constants.MasterRegionName, new Uri("ModuleWindow", UriKind.Relative));

        }

        public void NavigateToModuleLoaderView(IContainerProvider container)
        {
            var region = container.Resolve<IRegionManager>();
            if (region == null) return;

            if (!IsExistedNaviation(region, Constants.MasterRegionName, typeof(ModuleList)))
            {
                region.RegisterViewWithRegion(Constants.MasterRegionName, typeof(ModuleList));
            }

            region.RequestNavigate(Constants.MasterRegionName, new Uri("ModuleList", UriKind.Relative));
        }

        private bool IsExistedNaviation(IRegionManager regionManager, string regionName, Type viewType)
        {
            var selectedRegion = regionManager.Regions.FirstOrDefault(p => p.Name == regionName);
            if (selectedRegion == null) return false;

            var selectedView = selectedRegion.Views.FirstOrDefault(p => p.GetType().FullName == viewType.FullName);
            if (selectedView == null) return false;

            return true;
        }
    }
}

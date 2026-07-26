using PowerTools.Core.Configurations;
using PowerTools.Views;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Linq;
using System.Windows;
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

        public void NavigateToModuleView(ToolModule module)
        {
            var container = ModuleLoader.Container;
            var region = container.Resolve<IRegionManager>();
            if (region == null) return;

            ModuleGlobalSettings.Instance.CurrentModule = module;

            var viewType = ModuleViewSelectionHelper.GetView(module.Name);
            if (viewType == null)
            {
                return;
            }

            if (!IsExistedNavigation(region, Constants.ModuleRegionName, viewType))
            {
                region.RegisterViewWithRegion(Constants.ModuleRegionName, viewType);
            }

            region.RequestNavigate(Constants.ModuleRegionName, viewType.FullName);
        }

        public void NavigateToModuleLoaderView()
        {
            var container = ModuleLoader.Container;
            var region = container.Resolve<IRegionManager>();
            if (region == null) return;

            if (!IsExistedNavigation(region, Constants.ModuleRegionName, typeof(ModuleList)))
            {
                region.RegisterViewWithRegion(Constants.ModuleRegionName, typeof(ModuleList));
            }

            region.RequestNavigate(Constants.ModuleRegionName, new Uri("ModuleList", UriKind.Relative));
        }

        public void NavigateToDialogView(Type dialogViewType, DialogInformation dialogInformation)
        {
            var container = ModuleLoader.Container;
            var region = container.Resolve<IRegionManager>();
            if (region == null) return;

            if (!IsExistedNavigation(region, Constants.DialogRegionName, dialogViewType))
            {
                region.RegisterViewWithRegion(Constants.DialogRegionName, dialogViewType);
            }

            region.RequestNavigate(Constants.DialogRegionName, dialogViewType.FullName);
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

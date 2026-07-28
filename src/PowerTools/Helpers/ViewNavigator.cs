using PowerTools.Core.Configurations;
using PowerTools.Core.Models;
using PowerTools.Views;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Linq;

namespace PowerTools.Helpers
{
    public class ViewNavigator : BindableBase
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
            var region = ModuleLoader.RegionManager;

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
            var region = ModuleLoader.RegionManager;

            if (!IsExistedNavigation(region, Constants.ModuleRegionName, typeof(ModuleList)))
            {
                region.RegisterViewWithRegion(Constants.ModuleRegionName, typeof(ModuleList));
            }

            region.RequestNavigate(Constants.ModuleRegionName, new Uri("ModuleList", UriKind.Relative));
        }

        public void NavigateToDialogView(Type dialogViewType, DialogInformation dialogInformation)
        {
            var region = ModuleLoader.DialogRegionManager;

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

using PowerTools.Core.Configurations;
using PowerTools.Views;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using PowerTools.Core.Models;
using Prism.Mvvm;

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

        private IRegionManager _dialogRegionManager;
        public IRegionManager DialogRegionManager
        {
            get => _dialogRegionManager;
            set
            {
                _dialogRegionManager = value;
                RaisePropertyChanged(nameof(DialogRegionManager));
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

            var retryTimes = 10;
            while (retryTimes-- >= 0)
            {
                if (DialogRegionManager == null)
                {
                    DialogRegionManager = region.CreateRegionManager();
                }

                if (!DoRegisteredDialogView(dialogViewType))
                {
                    DialogRegionManager.RegisterViewWithRegion(Constants.DialogRegionName, dialogViewType);
                }

                try
                {
                    DialogRegionManager.RequestNavigate(Constants.DialogRegionName, dialogViewType.FullName);
                }
                catch (Exception e)
                {
                    _registeredDialogViews.Clear();
                    DialogRegionManager = null;
                }
            }
        }

        private List<Type> _registeredDialogViews = new List<Type>();
        private bool DoRegisteredDialogView(Type dialogView)
        {
            if (_registeredDialogViews.Contains(dialogView))
            {
                return true;
            }

            _registeredDialogViews.Add(dialogView);
            return false;
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

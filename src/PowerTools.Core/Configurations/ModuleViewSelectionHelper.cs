using System;
using System.Collections.Generic;

namespace PowerTools.Core.Configurations
{
    public class ModuleViewSelectionHelper
    {
        private static Dictionary<string, Type> _moduleViews = new Dictionary<string, Type>();

        public static void AddView(string moduleName, Type viewType)
        {
            if (_moduleViews.ContainsKey(moduleName))
            {
                _moduleViews[moduleName] = viewType;
            }
            else
            {
                _moduleViews.Add(moduleName, viewType);
            }
        }

        public static Type? GetView(string moduleName)
        {
            if (_moduleViews.ContainsKey(moduleName))
            {
                return _moduleViews[moduleName];
            }

            return null;
        }
    }
}

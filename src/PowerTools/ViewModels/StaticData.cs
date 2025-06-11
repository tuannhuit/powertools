using PowerTools.Core.Models;
using System.Collections.ObjectModel;

namespace PowerTools.ViewModels
{
    public class StaticData
    {
        public static ObservableCollection<ToolModule> ToolModules = new ObservableCollection<ToolModule>()
        {
            new ToolModule
            {
                Name = "Module 1",
                Description = "Module 1 is for Testing purpose",
                Version = "1.0.0.0"
            }, 
            new ToolModule
            {
                Name = "Module 2",
                Description = "Module 2 is for Testing purpose",
                Version = "1.0.0.0"
            }
        };

        public static ToolModule SelectedModule = new ToolModule()
        {
            Name = "Base64ToString Converter\"",
            Description = "A way to convert base64 string to Unicode string",
            Version = "1.0.0.0"
        };
    }
}

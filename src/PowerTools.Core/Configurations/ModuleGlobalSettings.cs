using System;
using System.IO;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using Prism.Mvvm;

namespace PowerTools.Core.Configurations
{
    public class ModuleGlobalSettings : BindableBase
    {
        private static ModuleGlobalSettings _instance;

        public static ModuleGlobalSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ModuleGlobalSettings();

                return _instance;
            }
        }

        private ModuleGlobalSettings()
        {
            RepositoryRemote = "\\\\hsnicx-fg01\\icxteamcitybucket\\Teams\\Delta\\Tools\\PowerTool\\modules";
        }

        public string RepositoryRemote { get; set; }

        public string RepositoryLocal => "modules";

        public string DataStoreLocal => "data";
        public string RepositoryFileName => "repository.json";

        private JsonNode? _currentModuleConfigurations;

        private ToolModule _currentModule;
        public ToolModule CurrentModule
        {
            get => _currentModule;
            set
            {
                _currentModule = value;
                RaisePropertyChanged("CurrentModule");
            }
        }


        public string GetOrCreateDataStoreLocal(ToolModule module)
        {
            if (module == null)
                throw new Exception("The module must be specified!");

            var localDataStorePath = module.LocalDataStorePath;

            Directory.CreateDirectory(localDataStorePath);

            return localDataStorePath;
        }

        private JsonNode? LoadModuleConfigurations(bool forceLoad = false)
        {
            if (_currentModuleConfigurations != null && !forceLoad)
                return _currentModuleConfigurations;
            
            var moduleDataStore = Instance.GetOrCreateDataStoreLocal(Instance.CurrentModule);

            var appConfigPath = Path.Combine(moduleDataStore, "appsettings.json");
            if (!File.Exists(appConfigPath))
            {
                _currentModuleConfigurations = new JsonObject();
                _currentModuleConfigurations!["appsettings"] = new JsonObject();
                File.WriteAllText(appConfigPath, _currentModuleConfigurations.ToJsonString());
            }

            try
            {
                var jsonData = File.ReadAllText(appConfigPath);
                _currentModuleConfigurations = JsonNode.Parse(jsonData);

                return _currentModuleConfigurations;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Info($"Error loading app configurations: {ex.Message}");
                return null;
            }

            return null;
        }

        public string LoadModuleConfigurationsAsString()
        {
            var appConfigurations = LoadModuleConfigurations();
            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.WriteIndented = true;

            return appConfigurations.ToJsonString(jsonSerializerOptions);
        }

        public void SaveModuleConfigurations(string configurations, bool cache = false)
        {
            var moduleDataStore = Instance.GetOrCreateDataStoreLocal(Instance.CurrentModule);
            var appConfigPath = Path.Combine(moduleDataStore, "appsettings.json");
            File.WriteAllText(appConfigPath, configurations);

            if (cache)
                LoadModuleConfigurations(true);
        }

        public string GetModuleConfigurationsByKey(string keyName)
        {
            var appConfigurations = LoadModuleConfigurations();

            if (appConfigurations == null)
                return string.Empty;

            var item = appConfigurations!["appsettings"]![keyName];
            if (item != null)
                return item.GetValue<string>();

            return string.Empty;
        }

        public void SaveModuleConfigurationsByKey(string keyName, string value)
        {
            var appConfigurations = LoadModuleConfigurations();

            if (appConfigurations == null)
                return;

            appConfigurations!["appsettings"]![keyName] = value;

            var jsonSerializerOptions = new JsonSerializerOptions();
            jsonSerializerOptions.WriteIndented = true;

            var updateSettings = appConfigurations.ToJsonString(jsonSerializerOptions);
            SaveModuleConfigurations(updateSettings, true);
        }
    }
}

using PowerTools.Core.Models;
using PowerTools.Core.SharedServices;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

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
            ResetWindowSettings();
        }
        public WindowSettings WindowSettings { get; set; }

        public string RepositoryRemote
        {
            get
            {
                var configs = LoadApplicationConfigurationsAsList();
                return configs.First(p => p.Key == "RepositoryRemote").Value;
            }
        }

        public string RepositoryLocalName => "modules";

        public string DataStoreLocalName => "data";
        public string RepositoryFileName => "repository.json";

        private JsonNode? _currentModuleConfigurations;
        private JsonNode? _currentApplicationConfigurations;

        private ToolModule _currentModule;
        public ToolModule CurrentModule
        {
            get => _currentModule;
            set
            {
                _currentModule = value;

                if (_currentModule == null)
                {
                    Repositories.RepositoryLocal.ModuleList.ForEach(p => p.IsSelected = false);
                }
                else
                {
                    _currentModule.IsSelected = true;
                    var remainingModules = Repositories.RepositoryLocal.ModuleList.Where(p => p != _currentModule);
                    foreach (var remainingModule in remainingModules)
                    {
                        remainingModule.IsSelected = false;
                    }
                }

                RaisePropertyChanged();
            }
        }

        public void ResetWindowSettings()
        {
            WindowSettings = new WindowSettings
            {
                MinWidth = 0,
                MinHeight = 0,
                MaxWidth = Double.PositiveInfinity,
                MaxHeight = Double.PositiveInfinity,
                Width = 1200,
                Height = 600,
            };
        }

        public string GetOrCreateDataStoreLocal()
        {
            if (Instance.CurrentModule != null)
            {
                return GetOrCreateDataStoreLocal(Instance.CurrentModule);
            }
            else
            {
                var assembly = Assembly.GetAssembly(this.GetType());
                return Path.GetDirectoryName(assembly.Location);
            }
        }

        public string GetOrCreateDataStoreLocal(ToolModule module)
        {
            if (module == null)
                throw new Exception("The module must be specified!");

            var localDataStorePath = module.DataStoreLocation;

            Directory.CreateDirectory(localDataStorePath);

            return localDataStorePath;
        }

        private JsonNode? LoadModuleConfigurations(bool forceLoad = false)
        {
            if (_currentModuleConfigurations != null && !forceLoad)
                return _currentModuleConfigurations;

            var moduleDataStore = GetOrCreateDataStoreLocal();

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
        }

        private JsonNode? LoadApplicationConfigurations(bool forceLoad = false)
        {
            if (_currentApplicationConfigurations != null && !forceLoad)
                return _currentApplicationConfigurations;

            var assembly = Assembly.GetAssembly(this.GetType());
            var appConfigPath = Path.Combine(Path.GetDirectoryName(assembly.Location), "appsettings.json");
            if (!File.Exists(appConfigPath))
            {
                _currentApplicationConfigurations = new JsonObject();
                _currentApplicationConfigurations!["appsettings"] = new JsonObject();
                File.WriteAllText(appConfigPath, _currentApplicationConfigurations.ToJsonString());
            }

            try
            {
                var jsonData = File.ReadAllText(appConfigPath);
                _currentApplicationConfigurations = JsonNode.Parse(jsonData);

                return _currentApplicationConfigurations;
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Info($"Error loading app configurations: {ex.Message}");
                return null;
            }
        }

        public List<KeyValuePair<string, string?>> LoadModuleConfigurationsAsList()
        {
            var appConfigurations = LoadModuleConfigurations();
            if (appConfigurations == null)
            {
                return new List<KeyValuePair<string, string?>>();
            }

            var settings = new List<KeyValuePair<string, string?>>();
            var configItems = appConfigurations["appsettings"]!.Deserialize<Dictionary<string, string?>>();
            if (configItems != null)
            {
                foreach (var item in configItems)
                {
                    settings.Add(new KeyValuePair<string, string?>(item.Key, item.Value));
                }
            }

            return settings;
        }

        public List<KeyValuePair<string, string?>> LoadApplicationConfigurationsAsList()
        {
            var appConfigurations = LoadApplicationConfigurations();
            if (appConfigurations == null)
            {
                return new List<KeyValuePair<string, string?>>();
            }

            var settings = new List<KeyValuePair<string, string?>>();
            var configItems = appConfigurations["appsettings"]!.Deserialize<Dictionary<string, string?>>();
            if (configItems != null)
            {
                foreach (var item in configItems)
                {
                    settings.Add(new KeyValuePair<string, string?>(item.Key, item.Value));
                }
            }

            if (settings.Count(p => p.Key == "RepositoryRemote") == 0)
            {
                settings.Add(new KeyValuePair<string, string?>("RepositoryRemote", null));
            }

            return settings;
        }

        public void SaveModuleConfigurations(Dictionary<string, string> configurations, bool cache = false)
        {
            var moduleDataStore = GetOrCreateDataStoreLocal();
            var appConfigPath = Path.Combine(moduleDataStore, "appsettings.json");

            dynamic settingObject = new ExpandoObject();
            settingObject.appsettings = configurations;
            File.WriteAllText(appConfigPath, JsonSerializer.Serialize(settingObject));

            if (cache)
            {
                if (CurrentModule != null)
                {
                    LoadModuleConfigurations(true);
                }
                else
                {
                    LoadApplicationConfigurations(true);
                }
            }
        }

        public void SaveApplicationConfigurations(Dictionary<string, string> configurations, bool cache = false)
        {
            var dataStore = Assembly.GetAssembly(this.GetType()).Location;
            var appConfigPath = Path.Combine(Path.GetDirectoryName(dataStore), "appsettings.json");

            dynamic settingObject = new ExpandoObject();
            settingObject.appsettings = configurations;
            File.WriteAllText(appConfigPath, JsonSerializer.Serialize(settingObject));

            if (cache)
                LoadApplicationConfigurations(true);
        }

        public void SaveModuleConfigurations(string configurations, bool cache = false)
        {
            var moduleDataStore = GetOrCreateDataStoreLocal();
            var appConfigPath = Path.Combine(moduleDataStore, "appsettings.json");
            File.WriteAllText(appConfigPath, configurations);

            if (cache)
                LoadModuleConfigurations(true);
        }

        public string GetModuleConfigurationsByKey(string keyName)
        {
            return GetModuleConfigurationsByKey<string>(keyName);
        }

        public T GetModuleConfigurationsByKey<T>(string keyName)
        {
            var appConfigurations = LoadModuleConfigurations();

            if (appConfigurations == null)
                return default(T);

            var item = appConfigurations!["appsettings"]![keyName];
            if (item != null)
                return item.GetValue<T>();

            return default(T);
        }

        public string GetModuleConfigurationsByKey(string keyName, string defaultValue)
        {
            var settingValue = GetModuleConfigurationsByKey(keyName);
            if (settingValue == null)
            {
                return defaultValue;
            }

            return settingValue;
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

using PowerTools.Core.Configurations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Prism.Mvvm;

namespace PowerTools.Core.Models
{
    public class ToolModule : BindableBase
    {
        /// <summary>
        /// Gets or sets tool name which is the tool identification
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the tool
        /// </summary>
        public string Description { get; set; }

        private string _version;
        /// <summary>
        /// Gets or sets the tool version which is running
        /// </summary>
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                RaisePropertyChanged("Version");
                RaisePropertyChanged("IsDownloaded");
            }
        }

        /// <summary>
        /// Checks if the current running tool version is download
        /// </summary>
        [JsonIgnore]
        public bool IsDownloaded => File.Exists(LocalModulePath);

        /// <summary>
        /// Gets or sets the list of versions of the tool
        /// </summary>
        public List<string> AllVersions { get; set; }

        [JsonIgnore]
        public string LatestVersion
        {
            get
            {
                if (AllVersions != null && AllVersions.Any())
                {
                    return AllVersions[AllVersions.Count() - 1];
                }

                return Version;
            }
        }

        /// <summary>
        /// Gets or sets the entry point of the tool
        /// </summary>
        public string ExecutionName { get; set; }

        /// <summary>
        /// Gets the local module location
        /// </summary>
        [JsonIgnore]
        public string LocalModulePath
        {
            get
            {
                var assembly = Assembly.GetAssembly(typeof(ToolModule));
                var modulePath = Path.Combine(Path.GetDirectoryName(assembly.Location), $"{ModuleGlobalSettings.Instance.RepositoryLocal}\\{Name}\\{Version}\\{ExecutionName}");

                return modulePath;
            }
        }

        /// <summary>
        /// Gets local data store location
        /// </summary>
        [JsonIgnore]
        public string LocalDataStorePath
        {
            get
            {
                var assembly = Assembly.GetAssembly(typeof(ToolModule));
                var modulePath = Path.Combine(Path.GetDirectoryName(assembly.Location), $"{ModuleGlobalSettings.Instance.DataStoreLocal}\\{Name}");

                return modulePath;
            }
        }
    }
}

using PowerTools.Core.Configurations;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace PowerTools.Core.Models
{
    public class ToolModule : BindableBase
    {
        /// <summary>
        /// The name of module which is the module identifier name
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        /// The display name of module
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets description of the tool
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the tool version which is running
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Checks if the current running tool version is download
        /// </summary>
        [JsonIgnore]
        public bool IsInstalled => File.Exists(ModuleLocation);

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

        private string _icon;
        public string Icon
        {
            get
            {
                if (string.IsNullOrEmpty(_icon) || string.IsNullOrWhiteSpace(_icon))
                {
                    return ButtonIcons.UnknownModule;
                }

                return _icon;
            }
            set
            {
                _icon = value;
                RaisePropertyChanged();
            }
        }
        public string IconImageRelativeLocation { get; set; }

        [JsonIgnore]
        public ModuleIconStyle IconStyle
        {
            get
            {
                if (string.IsNullOrEmpty(IconImageRelativeLocation)
                    || string.IsNullOrWhiteSpace(IconImageRelativeLocation)
                    || !File.Exists(Path.Combine(ModuleLocation, IconImageRelativeLocation)))
                {
                    return ModuleIconStyle.FontStyle;
                }

                return ModuleIconStyle.ImageStyle;
            }
        }

        /// <summary>
        /// Gets the local module location
        /// </summary>
        [JsonIgnore]
        public string ExecutionLocation => Path.Combine(ModuleLocation, $"{ExecutionName}");

        /// <summary>
        /// Gets the local module location
        /// </summary>
        [JsonIgnore]
        public string ModuleLocation
        {
            get
            {
                var assembly = Assembly.GetAssembly(typeof(ToolModule));
                var modulePath = Path.Combine(Path.GetDirectoryName(assembly.Location), $"{ModuleGlobalSettings.Instance.RepositoryLocal}\\{Name}-{Version}");

                return modulePath;
            }
        }

        /// <summary>
        /// Gets local data store location
        /// </summary>
        [JsonIgnore]
        public string DataStoreLocation
        {
            get
            {
                var assembly = Assembly.GetAssembly(typeof(ToolModule));
                var modulePath = Path.Combine(Path.GetDirectoryName(assembly.Location), $"{ModuleGlobalSettings.Instance.DataStoreLocal}\\{Name}");

                return modulePath;
            }
        }

        [JsonIgnore]
        private bool _isLoadedProperly;
        public bool IsLoadedProperly
        {
            get => _isLoadedProperly;
            set
            {
                _isLoadedProperly = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsLoadedFailed");
            }
        }

        [JsonIgnore] public bool IsLoadedFailed => !IsLoadedProperly;

        [JsonIgnore]
        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                RaisePropertyChanged();
            }
        }
    }
}

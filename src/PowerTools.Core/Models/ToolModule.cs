using PowerTools.Core.Configurations;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Windows.Media;
using PowerTools.Core.SharedServices;

namespace PowerTools.Core.Models
{
    public class ToolModule : BindableBase, ICloneable
    {
        /// <summary>
        /// The name of module which is the module identifier name
        /// </summary>
        public string Name { get; set; }

        private string _displayName;
        /// <summary> 
        /// The display name of module
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_displayName) || string.IsNullOrEmpty(_displayName))
                {
                    return Name;
                }

                return _displayName;
            }
            set
            {
                _displayName = value;
                RaisePropertyChanged();
            }
        }

        private string _publisherDisplayName;
        /// <summary> 
        /// The display name of module
        /// </summary>
        public string PublisherDisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_publisherDisplayName) || string.IsNullOrEmpty(_publisherDisplayName))
                {
                    return "Unknown Publisher";
                }

                return _publisherDisplayName;
            }
            set
            {
                _publisherDisplayName = value;
                RaisePropertyChanged();
            }
        }

        private string _repoLink;

        public string RepoLink
        {
            get => _repoLink;
            set
            {
                _repoLink = value;
                RaisePropertyChanged();
            }
        }


        private string _description;
        /// <summary>
        /// Gets or sets description of the tool
        /// </summary>
        public string Description
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_description) || string.IsNullOrEmpty(_description))
                {
                    return "No Description";
                }

                return _description;
            }
            set
            {
                _description = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the tool version which is running
        /// </summary>
        private string _version;
        public string Version
        {
            get => _version;
            set
            {
                _version = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsInstalled");
                RaisePropertyChanged("IsNotInstalled");
                RaisePropertyChanged("IsActive");
            }
        }

        /// <summary>
        /// Checks if the current running tool version is download
        /// </summary>
        [JsonIgnore]
        public bool IsInstalled => File.Exists(ExecutionLocation);

        /// <summary>
        /// Checks if the current running tool version is download
        /// </summary>
        [JsonIgnore]
        public bool IsNotInstalled => !IsInstalled;

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

        private string _iconColor;
        public string IconColor
        {
            get => _iconColor;
            set
            {
                _iconColor = value;
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
                var modulePath = Path.Combine(Path.GetDirectoryName(assembly.Location), $"{ModuleGlobalSettings.Instance.RepositoryLocalName}\\{Name}-{Version}");

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
                var modulePath = Path.Combine(Path.GetDirectoryName(assembly.Location), $"{ModuleGlobalSettings.Instance.DataStoreLocalName}\\{Name}");

                return modulePath;
            }
        }

        private bool _isLoadedProperly;

        [JsonIgnore]
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

        [JsonIgnore]
        public bool IsLoadedFailed => !IsLoadedProperly;

        private bool _isDownloading;
        [JsonIgnore]
        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                _isDownloading = value;
                RaisePropertyChanged();
            }
        }

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

        private bool _isMarkDeleted;
        public bool IsMarkDeleted
        {
            get => _isMarkDeleted;
            set
            {
                _isMarkDeleted = value;
                RaisePropertyChanged();
            }
        }

        private bool _isLoaded;
        [JsonIgnore]
        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                _isLoaded = value;
                RaisePropertyChanged();
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged();
            }
        }

        public object Clone()
        {
            return new ToolModule
            {
                Name = Name,
                DisplayName = DisplayName,
                PublisherDisplayName = PublisherDisplayName,
                RepoLink = RepoLink,
                Description = Description,
                Version = Version,
                AllVersions = AllVersions,
                ExecutionName = ExecutionName,
                Icon = Icon,
                IconImageRelativeLocation = IconImageRelativeLocation,
                IsLoadedProperly = IsLoadedProperly,
                IsActive = IsActive,
                IsSelected = IsSelected,
                IsLoaded = IsLoaded,
                IsMarkDeleted = IsMarkDeleted,
                IsDownloading = IsDownloading,
                IconColor = IconColor
            };
        }
    }
}

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
            get
            {
                if (string.IsNullOrWhiteSpace(_repoLink) || string.IsNullOrEmpty(_repoLink) || _repoLink == "No Repository Link")
                {
                    switch (RepoType)
                    {
                        case "github":
                            return $"https://github.com/{OwnerName}/{RepoName}";
                        case "gitlab":
                            return $"https://gitlab.com/{OwnerName}/{RepoName}";
                        case "bitbucket":
                            return $"https://bitbucket.org/{OwnerName}/{RepoName}";
                        default:
                            return "No Repository Link";
                    }
                }

                return _repoLink;
            }
            set
            {
                _repoLink = value;
                RaisePropertyChanged();
            }
        }

        private string _ownerName;
        public string OwnerName
        {
            get => _ownerName;
            set
            {
                _ownerName = value;
                RaisePropertyChanged();
                RaisePropertyChanged("RepoDisplayName");
            }
        }

        private string _repoName;
        public string RepoName
        {
            get => _repoName;
            set
            {
                _repoName = value;
                RaisePropertyChanged();
                RaisePropertyChanged("RepoDisplayName");
            }
        }

        public string RepoDisplayName => $"{OwnerName}/{RepoName}";

        private string _repoType;
        public string RepoType
        {
            get => _repoType;
            set
            {
                _repoType = value;
                RaisePropertyChanged();
            }
        }

        public string IconImageRelativeLocation { get; set; }

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
                RaisePropertyChanged("IsActive");
            }
        }

        private string _newVersion;
        /// <summary>
        /// Gets or sets the new tool version which should be installed
        /// </summary>
        [JsonIgnore]
        public string NewVersion
        {
            get => _newVersion;
            set
            {
                _newVersion = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Checks if the current running tool version is download
        /// </summary>
        [JsonIgnore]
        public bool IsVersionAllocated
        {
            get
            {
                if (string.IsNullOrEmpty(Version))
                    return false;

                return File.Exists(ExecutionLocation);
            }
        }

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
                if (assembly == null)
                {
                    throw new Exception("Not found assembly for ToolModule");
                }

                var assemblyLocation = Path.GetDirectoryName(assembly.Location);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    throw new Exception("Not found assembly location for ToolModule");
                }

                var modulePath = Path.Combine(assemblyLocation, $"{ModuleGlobalSettings.Instance.RepositoryLocalName}\\{Name}-{Version}");

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
                if (assembly == null)
                {
                    throw new Exception("Not found assembly for ToolModule");
                }

                var assemblyLocation = Path.GetDirectoryName(assembly.Location);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    throw new Exception("Not found assembly location for ToolModule");
                }

                var modulePath = Path.Combine(assemblyLocation, $"{ModuleGlobalSettings.Instance.DataStoreLocalName}\\{Name}");

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

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                RaisePropertyChanged();
                RaisePropertyChanged("IsNotActive");
            }
        }

        [JsonIgnore]
        public bool IsNotActive => !IsActive;

        private List<string> _markedUninstalledVersions = new List<string>();
        public List<string> MarkedUninstalledVersions
        {
            get => _markedUninstalledVersions;
            set
            {
                _markedUninstalledVersions = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public List<string> UninstalledVersionLocations
        {
            get
            {
                if (_markedUninstalledVersions == null || !_markedUninstalledVersions.Any())
                {
                    return new List<string>();
                }

                var uninstalledVersions = _markedUninstalledVersions.Distinct().ToArray();
                if (!uninstalledVersions.Any())
                {
                    return new List<string>();
                }

                var assembly = Assembly.GetAssembly(typeof(ToolModule));
                if (assembly == null)
                {
                    return new List<string>();
                }

                var assemblyLocation = Path.GetDirectoryName(assembly.Location);
                if (string.IsNullOrEmpty(assemblyLocation))
                {
                    return new List<string>();
                }

                return uninstalledVersions.Select(p => Path.Combine(assemblyLocation, $"{ModuleGlobalSettings.Instance.RepositoryLocalName}\\{Name}-{p}")).ToList();
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

        private VersionUpdateStatus _versionUpdateStatus;
        [JsonIgnore]
        public VersionUpdateStatus VersionUpdateStatus
        {
            get => _versionUpdateStatus;
            set
            {
                _versionUpdateStatus = value;
                RaisePropertyChanged();
            }
        }

        public bool TokenRequired { get; set; }

        [JsonIgnore]
        public string Token
        {
            get
            {
                if (!TokenRequired)
                {
                    return null;
                }

                string token = ModuleGlobalSettings.Instance.GetApplicationConfigurationsByKey($"{RepoType}.token.{RepoName}");
                if (string.IsNullOrEmpty(token))
                {
                    token = ModuleGlobalSettings.Instance.GetApplicationConfigurationsByKey($"{RepoType}.token");
                }

                return string.IsNullOrEmpty(token) ? null : token;
            }
        }

        public object Clone()
        {
            return new ToolModule
            {
                Name = Name,
                DisplayName = DisplayName,
                PublisherDisplayName = PublisherDisplayName,
                OwnerName = OwnerName,
                RepoName = RepoName,
                RepoType = RepoType,
                RepoLink = RepoLink,
                Description = Description,
                Version = Version,
                ExecutionName = ExecutionName,
                Icon = Icon,
                IconImageRelativeLocation = IconImageRelativeLocation,
                IsLoadedProperly = IsLoadedProperly,
                IsActive = IsActive,
                IsSelected = IsSelected,
                IsLoaded = IsLoaded,
                MarkedUninstalledVersions = MarkedUninstalledVersions,
                IconColor = IconColor,
                VersionUpdateStatus = VersionUpdateStatus,
                NewVersion = NewVersion,
                TokenRequired = TokenRequired
            };
        }
    }
}

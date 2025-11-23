using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PowerTools.Core.Models
{
    public class RemoteToolModule
    {
        /// <summary>
        /// The name of module which is the module identifier name
        /// </summary>
        public string Name { get; set; }

        /// <summary> 
        /// The display name of module
        /// </summary>
        public string DisplayName { get; set; }

        public string PublisherDisplayName { get; set; }
        public string RepoLink { get; set; }

        /// <summary>
        /// Gets or sets description of the tool
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entry point of the tool
        /// </summary>
        public string ExecutionName { get; set; }

        public string Icon { get; set; }

        public string IconColor { get; set; }

        public string IconImageRelativeLocation { get; set; }

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
                    return AllVersions.First();
                }

                return string.Empty;
            }
        }
    }
}

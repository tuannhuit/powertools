using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PowerTools.Core.Models
{
    public class RemoteToolModule
    {
        /// <summary>
        /// Gets or sets tool name which is the tool identification
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets description of the tool
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entry point of the tool
        /// </summary>
        public string ExecutionName { get; set; }

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

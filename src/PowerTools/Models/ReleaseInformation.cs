using System.Text.Json.Serialization;

namespace PowerTools.Models
{
    public class ReleaseInformation
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }


        [JsonPropertyName("assets")]
        public ReleaseAsset[] Assets { get; set; }
    }
}

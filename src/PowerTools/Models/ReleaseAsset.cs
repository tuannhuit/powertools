using System.Text.Json.Serialization;

namespace PowerTools.Models
{
    public class ReleaseAsset
    {
        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }
    }
}

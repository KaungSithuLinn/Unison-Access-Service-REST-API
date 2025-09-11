using System.Text.Json.Serialization;

namespace UnisonRestAdapter.Models.Response
{
    /// <summary>
    /// Response model for system version information
    /// </summary>
    public class VersionResponse
    {
        /// <summary>
        /// Indicates if the operation was successful
        /// </summary>
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Operation message
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// API adapter version
        /// </summary>
        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Backend service version from SOAP service
        /// </summary>
        [JsonPropertyName("backendVersion")]
        public string? BackendVersion { get; set; }

        /// <summary>
        /// System build information
        /// </summary>
        [JsonPropertyName("buildInfo")]
        public BuildInfo BuildInfo { get; set; } = new();

        /// <summary>
        /// Response timestamp
        /// </summary>
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Build information details
    /// </summary>
    public class BuildInfo
    {
        /// <summary>
        /// Build timestamp
        /// </summary>
        [JsonPropertyName("buildDate")]
        public DateTime BuildDate { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Runtime environment
        /// </summary>
        [JsonPropertyName("environment")]
        public string Environment { get; set; } = "Development";

        /// <summary>
        /// .NET runtime version
        /// </summary>
        [JsonPropertyName("runtimeVersion")]
        public string RuntimeVersion { get; set; } = System.Environment.Version.ToString();
    }
}

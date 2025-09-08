using UnisonRestAdapter.Services;
using Microsoft.Extensions.Options;

namespace UnisonRestAdapter.Configuration
{
    /// <summary>
    /// Configuration settings for Unison Access Service
    /// </summary>
    public class UnisonSettings
    {
        public const string SectionName = "Unison";

        /// <summary>
        /// SOAP service endpoint URL
        /// </summary>
        public string ServiceUrl { get; set; } = "http://192.168.10.206:9003/Unison.AccessService";

        /// <summary>
        /// WSDL endpoint URL
        /// </summary>
        public string WsdlUrl { get; set; } = "http://192.168.10.206:9003/Unison.AccessService?wsdl";

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int TimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Number of retry attempts for failed requests
        /// </summary>
        public int RetryAttempts { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts in milliseconds
        /// </summary>
        public int RetryDelayMs { get; set; } = 1000;

        /// <summary>
        /// Enable detailed logging for SOAP requests/responses
        /// </summary>
        public bool EnableDetailedLogging { get; set; } = false;
    }

    /// <summary>
    /// Service configuration and dependency injection setup
    /// </summary>
    public static class ServiceConfiguration
    {
        /// <summary>
        /// Configures services for dependency injection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configuration">Application configuration</param>
        public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Configure Unison settings
            services.Configure<UnisonSettings>(configuration.GetSection(UnisonSettings.SectionName));

            // Register application services
            services.AddScoped<IUnisonService, UnisonService>();
            services.AddScoped<ISoapClientService, SoapClientService>();

            // Configure HTTP client for SOAP service
            services.AddHttpClient("UnisonSoapClient", client =>
            {
                var unisonSettings = configuration.GetSection(UnisonSettings.SectionName).Get<UnisonSettings>() ?? new UnisonSettings();
                client.BaseAddress = new Uri(unisonSettings.ServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(unisonSettings.TimeoutSeconds);
            });

            // Add logging
            services.AddLogging();

            // Add health checks
            services.AddHealthChecks()
                .AddCheck<UnisonHealthCheck>("unison_service");
        }
    }
}

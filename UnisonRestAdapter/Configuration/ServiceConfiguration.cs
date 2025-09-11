using UnisonRestAdapter.Services;
using UnisonRestAdapter.Services.Monitoring;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace UnisonRestAdapter.Configuration
{
    /// <summary>
    /// Configuration settings for Unison Access Service
    /// </summary>
    public class UnisonSettings
    {
        /// <summary>
        /// Configuration section name
        /// </summary>
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

            // Configure HTTP client options for performance optimization
            services.Configure<HttpClientOptions>(configuration.GetSection(HttpClientOptions.SectionName));

            // Register application services
            services.AddScoped<IUnisonService, UnisonService>();
            services.AddScoped<ISoapClientService, SoapClientService>();
            services.AddScoped<IResponseCacheService, ResponseCacheService>();
            services.AddSingleton<IPerformanceMonitoringService, PerformanceMonitoringService>();
            services.AddSingleton<IMonitoringService, MonitoringService>();

            // Add memory cache for response caching
            services.AddMemoryCache();

            // Configure high-performance HTTP client for SOAP service
            services.AddHttpClient("UnisonSoapClient", (serviceProvider, client) =>
            {
                var unisonSettings = configuration.GetSection(UnisonSettings.SectionName).Get<UnisonSettings>() ?? new UnisonSettings();
                var httpClientOptions = configuration.GetSection(HttpClientOptions.SectionName).Get<HttpClientOptions>() ?? new HttpClientOptions();

                client.BaseAddress = new Uri(unisonSettings.ServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(httpClientOptions.RequestTimeoutSeconds);

                // Set keep-alive headers for connection pooling
                client.DefaultRequestHeaders.Connection.Add("keep-alive");
                client.DefaultRequestHeaders.Add("Keep-Alive", $"timeout={httpClientOptions.TcpKeepAliveTime}");

            })
            .ConfigurePrimaryHttpMessageHandler((serviceProvider) =>
            {
                var httpClientOptions = configuration.GetSection(HttpClientOptions.SectionName).Get<HttpClientOptions>() ?? new HttpClientOptions();

                var handler = new SocketsHttpHandler
                {
                    // Connection pooling configuration
                    MaxConnectionsPerServer = httpClientOptions.MaxConnectionsPerEndpoint,
                    PooledConnectionLifetime = TimeSpan.FromSeconds(httpClientOptions.PooledConnectionLifetimeSeconds),
                    PooledConnectionIdleTimeout = TimeSpan.FromSeconds(httpClientOptions.PooledConnectionIdleTimeoutSeconds),

                    // Keep-alive configuration
                    EnableMultipleHttp2Connections = true,

                    // Timeout configuration
                    ConnectTimeout = TimeSpan.FromSeconds(httpClientOptions.ConnectionTimeoutSeconds),

                    // Redirect configuration
                    AllowAutoRedirect = true,
                    MaxAutomaticRedirections = httpClientOptions.MaxAutomaticRedirections,

                    // Performance optimizations
                    UseCookies = false // Disable cookies for better performance in stateless scenarios
                };

                // Configure TCP keep-alive if supported
                if (httpClientOptions.EnableKeepAlive)
                {
                    handler.SslOptions.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                }

                return handler;
            });

            // Add logging
            services.AddLogging();

            // Add health checks
            services.AddHealthChecks()
                .AddCheck<UnisonHealthCheck>("unison_service");
        }
    }
}

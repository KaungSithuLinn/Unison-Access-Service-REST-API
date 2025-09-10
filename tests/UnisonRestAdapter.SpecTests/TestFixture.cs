#nullable enable
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using UnisonRestAdapter.Services;
using UnisonRestAdapter.Security;
using UnisonRestAdapter.Configuration;

namespace UnisonRestAdapter.SpecTests
{
    public class TestFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace IUnisonService with fake implementation
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUnisonService));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddScoped<IUnisonService, FakeUnisonService>();

                // Ensure security services are properly registered for testing
                services.Configure<SecurityOptions>(options =>
                {
                    options.ValidTokens = new List<string> { "595d799a-9553-4ddf-8fd9-c27b1f233ce7" };
                    options.EnableTokenRotation = false;
                    options.TokenExpiryHours = 24;
                    options.EncryptTokensInStorage = false;
                    options.AllowFallbackToken = true;
                    options.FallbackToken = "595d799a-9553-4ddf-8fd9-c27b1f233ce7";
                    options.LogSecurityEvents = true;
                });

                // Ensure ITokenService is registered if not already present
                var tokenServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITokenService));
                if (tokenServiceDescriptor == null)
                {
                    services.AddScoped<ITokenService, TokenService>();
                }
            });

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                // Add test configuration
                var testConfig = new Dictionary<string, string?>
                {
                    { "Security:ValidTokens:0", "595d799a-9553-4ddf-8fd9-c27b1f233ce7" },
                    { "Security:EnableTokenRotation", "false" },
                    { "Security:TokenExpiryHours", "24" },
                    { "Security:EncryptTokensInStorage", "false" },
                    { "Security:AllowFallbackToken", "true" },
                    { "Security:FallbackToken", "595d799a-9553-4ddf-8fd9-c27b1f233ce7" }
                };

                configBuilder.AddInMemoryCollection(testConfig);
            });
        }
    }
}

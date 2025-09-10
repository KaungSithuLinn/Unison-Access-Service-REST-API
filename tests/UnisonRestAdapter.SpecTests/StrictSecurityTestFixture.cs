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
    /// <summary>
    /// Test fixture with strict security (no fallback tokens)
    /// </summary>
    public class StrictSecurityTestFixture : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the existing Unison service and replace with fake
                var unisonServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IUnisonService));
                if (unisonServiceDescriptor != null)
                {
                    services.Remove(unisonServiceDescriptor);
                }
                services.AddScoped<IUnisonService, FakeUnisonService>();

                // Ensure ITokenService is registered if not already present
                var tokenServiceDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(ITokenService));
                if (tokenServiceDescriptor == null)
                {
                    services.AddScoped<ITokenService, TokenService>();
                }
            });

            builder.ConfigureAppConfiguration((context, configBuilder) =>
            {
                // Add strict test configuration (no fallback token)
                var testConfig = new Dictionary<string, string?>
                {
                    { "Security:ValidTokens:0", "595d799a-9553-4ddf-8fd9-c27b1f233ce7" },
                    { "Security:EnableTokenRotation", "false" },
                    { "Security:TokenExpiryHours", "24" },
                    { "Security:EncryptTokensInStorage", "false" },
                    { "Security:AllowFallbackToken", "false" },
                    { "Security:FallbackToken", "" }
                };

                configBuilder.AddInMemoryCollection(testConfig);
            });
        }
    }
}

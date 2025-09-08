using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using UnisonRestAdapter.Services;

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
            });
        }
    }
}

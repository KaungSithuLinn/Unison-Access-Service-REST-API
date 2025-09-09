using Microsoft.OpenApi.Models;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Security;
using UnisonRestAdapter.Middleware;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Configure logging for Windows Service
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Windows Event Log only on Windows platform
if (OperatingSystem.IsWindows())
{
    builder.Logging.AddEventLog(settings =>
    {
        if (OperatingSystem.IsWindows())
        {
            settings.SourceName = "UnisonRestAdapter";
            settings.LogName = "Application";
        }
    });
}

// Configure as Windows Service if running as service
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "UnisonRestAdapter";
});

// Add services to the container.
builder.Services.AddControllers();

// Configure Unison adapter services
ServiceConfiguration.ConfigureServices(builder.Services, builder.Configuration);

// Configure security services
builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
builder.Services.AddScoped<ITokenService, TokenService>();

// Add health checks
builder.Services.AddHealthChecks(); ;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Unison REST Adapter API",
        Version = "v1",
        Description = "REST-to-SOAP adapter for Unison Access Service",
        Contact = new OpenApiContact
        {
            Name = "Unison Support",
            Email = "support@company.com"
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Add authentication header to Swagger
    c.AddSecurityDefinition("UnisonToken", new OpenApiSecurityScheme
    {
        Description = "Unison authentication token",
        Name = "Unison-Token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "UnisonToken"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "UnisonToken"
                }
            },
            new string[] {}
        }
    });
});

var app = builder.Build();

// Get logger for startup/shutdown events
var logger = app.Services.GetRequiredService<ILogger<Program>>();

// Log service startup
logger.LogInformation("Unison REST Adapter service starting up");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger in production for API documentation
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Unison REST Adapter API v1");
        c.RoutePrefix = "api/docs"; // Serve at /api/docs instead of root
    });
}

// Add health checks endpoint
app.MapHealthChecks("/health");

// Add error handling middleware (must be first in pipeline)
app.UseMiddleware<ErrorHandlingMiddleware>();

// Add token validation middleware
app.UseMiddleware<TokenValidationMiddleware>();

app.UseHttpsRedirection(); ;

app.UseAuthorization();

app.MapControllers();

// Add graceful shutdown handling
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    logger.LogInformation("Unison REST Adapter service stopping");
});

logger.LogInformation("Unison REST Adapter service started successfully on port 5203");

app.Run();

/// <summary>
/// Program class for the Unison REST Adapter service
/// </summary>
public partial class Program { }

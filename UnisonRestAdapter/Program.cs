using Microsoft.OpenApi.Models;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Security;
using UnisonRestAdapter.Middleware;
using UnisonRestAdapter.Services.Tracing;
using System.Diagnostics;
using Serilog;
using Serilog.Context;
using Serilog.Events;

// Configure Serilog first to capture startup logs
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting Unison REST Adapter service...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from configuration
    builder.Host.UseSerilog((context, services, configuration) =>
    {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", "UnisonRestAdapter")
            .Enrich.WithProperty("Version", "1.0.0");

        // Add Windows Event Log only on Windows platform and if running as service or with admin rights
        if (OperatingSystem.IsWindows() && !builder.Environment.IsDevelopment())
        {
            try
            {
                configuration.WriteTo.EventLog(
                    source: "UnisonRestAdapter",
                    logName: "Application",
                    manageEventSource: false); // Don't try to create event source
            }
            catch (System.Security.SecurityException)
            {
                // Ignore if we don't have permissions to write to event log
                Log.Warning("Event log sink not available due to insufficient permissions");
            }
        }
    });

    // Configure as Windows Service if running as service
    builder.Host.UseWindowsService(options =>
    {
        options.ServiceName = "UnisonRestAdapter";
    });

    // Add services to the container.
    builder.Services.AddControllers();

    // Add HTTP context accessor for correlation service
    builder.Services.AddHttpContextAccessor();

    // Configure request tracing options
    builder.Services.Configure<RequestTracingOptions>(
        builder.Configuration.GetSection(RequestTracingOptions.SectionName));

    // Register tracing services
    builder.Services.AddScoped<ICorrelationService, CorrelationService>();

    // Configure Unison adapter services
    ServiceConfiguration.ConfigureServices(builder.Services, builder.Configuration);

    // Configure security services
    builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
    builder.Services.AddScoped<ITokenService, TokenService>();

    // Add memory cache for rate limiting
    builder.Services.AddMemoryCache();

    // Configure CORS
    var securityOptions = builder.Configuration.GetSection(SecurityOptions.SectionName).Get<SecurityOptions>();
    if (securityOptions?.EnableCors == true)
    {
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (securityOptions.AllowedOrigins?.Any() == true)
                {
                    policy.WithOrigins(securityOptions.AllowedOrigins.ToArray());
                }
                else
                {
                    policy.AllowAnyOrigin();
                }

                if (securityOptions.AllowedMethods?.Any() == true)
                {
                    policy.WithMethods(securityOptions.AllowedMethods.ToArray());
                }
                else
                {
                    policy.AllowAnyMethod();
                }

                if (securityOptions.AllowedHeaders?.Any() == true)
                {
                    policy.WithHeaders(securityOptions.AllowedHeaders.ToArray());
                }
                else
                {
                    policy.AllowAnyHeader();
                }
            });
        });
    }

    // Add health checks
    builder.Services.AddHealthChecks();

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

    // Configure Serilog middleware
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.GetLevel = (httpContext, elapsed, ex) => ex != null ? LogEventLevel.Error : LogEventLevel.Information;
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.FirstOrDefault());
            diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress?.ToString());

            // Add correlation ID to diagnostic context
            if (httpContext.Items.TryGetValue("CorrelationId", out var correlationId))
            {
                diagnosticContext.Set("CorrelationId", correlationId);
            }
        };
    });

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

    // Configure CORS if enabled
    var securityConfig = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<SecurityOptions>>().Value;
    if (securityConfig.EnableCors)
    {
        app.UseCors();
    }

    // Add security middleware pipeline (in order of execution)
    // 1. Security headers (first for all responses)
    app.UseSecurityHeaders();

    // 2. IP whitelist check (early security gate)
    if (securityConfig.EnableIpWhitelist)
    {
        app.UseIpWhitelist();
    }

    // 3. Rate limiting (after IP check, before expensive operations)
    if (securityConfig.EnableRateLimiting)
    {
        app.UseMiddleware<UnisonRestAdapter.Security.RateLimitingMiddleware>();
    }

    // 4. Request validation (after rate limiting, before processing)
    if (securityConfig.EnableRequestValidation)
    {
        app.UseMiddleware<UnisonRestAdapter.Security.RequestValidationMiddleware>();
    }

    // Add request logging middleware (after security checks)
    app.UseMiddleware<RequestLoggingMiddleware>();

    // Add performance monitoring middleware (after request logging)
    app.UseMiddleware<PerformanceMonitoringMiddleware>();

    // Add metrics collection middleware (after performance monitoring)
    app.UseMiddleware<UnisonRestAdapter.Middleware.MetricsCollectionMiddleware>();

    // Add error handling middleware (after request logging and performance monitoring)
    app.UseMiddleware<ErrorHandlingMiddleware>();

    // Add token validation middleware (after error handling, before business logic)
    app.UseMiddleware<TokenValidationMiddleware>();

    app.UseHttpsRedirection();

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

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}

/// <summary>
/// Program class for the Unison REST Adapter service
/// </summary>
public partial class Program { }

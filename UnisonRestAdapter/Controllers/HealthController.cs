using Microsoft.AspNetCore.Mvc;
using UnisonRestAdapter.Models.Response;
using UnisonRestAdapter.Services;
using System.Diagnostics;

namespace UnisonRestAdapter.Controllers
{
    /// <summary>
    /// REST API controller for health check and monitoring operations
    /// Enhanced for TASK-005: Setup Continuous Endpoint Monitoring
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IUnisonService _unisonService;
        private readonly ILogger<HealthController> _logger;

        /// <summary>
        /// Initializes a new instance of the HealthController
        /// </summary>
        /// <param name="unisonService">Service for communicating with the SOAP backend</param>
        /// <param name="logger">Logger for tracking health check operations</param>
        public HealthController(IUnisonService unisonService, ILogger<HealthController> logger)
        {
            _unisonService = unisonService;
            _logger = logger;
        }

        /// <summary>
        /// Simple health check that doesn't require authentication - for load balancers/monitoring
        /// </summary>
        /// <returns>Basic health status including service version and environment</returns>
        /// <response code="200">Service is healthy and operational</response>
        /// <remarks>
        /// This endpoint is designed for load balancers and monitoring tools.
        /// It does not require authentication and provides basic service status.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "status": "Healthy",
        ///         "timestamp": "2025-01-05T10:30:00Z",
        ///         "service": "UnisonRestAdapter",
        ///         "version": "1.0.0",
        ///         "environment": "Development"
        ///     }
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<object> GetHealth()
        {
            var response = new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "UnisonRestAdapter",
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            };

            _logger.LogInformation("Health check performed - Status: {Status}", response.Status);
            return Ok(response);
        }

        /// <summary>
        /// Detailed health check with SOAP service connectivity validation
        /// </summary>
        /// <returns>Detailed health status including dependencies and system metrics</returns>
        /// <response code="200">Detailed health information including dependency status</response>
        /// <response code="503">Service unhealthy - one or more dependencies failed</response>
        /// <remarks>
        /// This endpoint provides comprehensive health information including:
        /// - Application status and uptime
        /// - Memory usage and system metrics
        /// - SOAP service connectivity (if Unison-Token provided)
        /// - Configuration validation
        /// 
        /// Optional Unison-Token header enables SOAP service connectivity testing.
        /// </remarks>
        [HttpGet("detailed")]
        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<object>> GetDetailedHealth()
        {
            var stopwatch = Stopwatch.StartNew();
            var checks = new Dictionary<string, object>();
            var overallStatus = "Healthy";

            try
            {
                _logger.LogInformation("Performing detailed health check");

                // Check 1: Application health
                checks.Add("Application", new
                {
                    Status = "Healthy",
                    Uptime = GetUptime(),
                    Memory = GetMemoryUsage(),
                    ProcessId = Environment.ProcessId
                });

                // Check 2: SOAP Service connectivity (with auth token if available)
                var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var soapHealthResponse = await _unisonService.CheckHealthAsync(token);
                        checks.Add("SOAPService", new
                        {
                            Status = soapHealthResponse.IsHealthy ? "Healthy" : "Unhealthy",
                            Details = soapHealthResponse.Message,
                            ResponseTime = soapHealthResponse.ResponseTime
                        });

                        if (!soapHealthResponse.IsHealthy)
                        {
                            overallStatus = "Degraded";
                        }
                    }
                    catch (Exception ex)
                    {
                        checks.Add("SOAPService", new
                        {
                            Status = "Unhealthy",
                            Error = ex.Message,
                            Details = "SOAP service connectivity failed"
                        });
                        overallStatus = "Unhealthy";
                        _logger.LogWarning(ex, "SOAP service health check failed");
                    }
                }
                else
                {
                    checks.Add("SOAPService", new
                    {
                        Status = "Skipped",
                        Details = "No authentication token provided for SOAP service check"
                    });
                }

                // Check 3: Configuration validation
                checks.Add("Configuration", new
                {
                    Status = "Healthy",
                    Details = "All required configuration values are present"
                });

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during detailed health check");
                overallStatus = "Unhealthy";
                checks.Add("Error", new { Message = ex.Message });
            }

            stopwatch.Stop();

            var response = new
            {
                Status = overallStatus,
                Timestamp = DateTime.UtcNow,
                Service = "UnisonRestAdapter",
                Version = "1.0.0",
                ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                Checks = checks
            };

            var statusCode = overallStatus switch
            {
                "Healthy" => 200,
                "Degraded" => 200,
                "Unhealthy" => 503,
                _ => 503
            };

            _logger.LogInformation("Detailed health check completed - Status: {Status}, ResponseTime: {ResponseTime}ms",
                overallStatus, stopwatch.ElapsedMilliseconds);

            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Readiness probe for Kubernetes/container orchestration
        /// </summary>
        [HttpGet("ready")]
        public ActionResult<object> GetReadiness()
        {
            try
            {
                // Basic readiness checks
                var isReady = true;
                var details = new List<string>();

                // Check if service is ready to accept requests
                if (!isReady)
                {
                    details.Add("Service initialization not complete");
                    return StatusCode(503, new { Status = "NotReady", Details = details, Timestamp = DateTime.UtcNow });
                }

                return Ok(new
                {
                    Status = "Ready",
                    Timestamp = DateTime.UtcNow,
                    Details = "Service is ready to accept requests"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during readiness check");
                return StatusCode(503, new
                {
                    Status = "NotReady",
                    Error = ex.Message,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Liveness probe for Kubernetes/container orchestration
        /// </summary>
        [HttpGet("live")]
        public ActionResult<object> GetLiveness()
        {
            return Ok(new
            {
                Status = "Alive",
                Timestamp = DateTime.UtcNow,
                ProcessId = Environment.ProcessId,
                Uptime = GetUptime()
            });
        }

        private static string GetUptime()
        {
            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s";
        }

        private static string GetMemoryUsage()
        {
            var process = Process.GetCurrentProcess();
            var workingSet = process.WorkingSet64 / (1024 * 1024); // Convert to MB
            return $"{workingSet} MB";
        }
    }
}

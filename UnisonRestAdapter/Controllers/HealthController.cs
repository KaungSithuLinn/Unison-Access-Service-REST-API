using Microsoft.AspNetCore.Mvc;
using UnisonRestAdapter.Models.Response;
using UnisonRestAdapter.Models.Monitoring;
using UnisonRestAdapter.Services;
using UnisonRestAdapter.Services.Monitoring;
using System.Diagnostics;

namespace UnisonRestAdapter.Controllers
{
    /// <summary>
    /// REST API controller for health check and monitoring operations
    /// Enhanced for Issue #6: Implement Monitoring and Health Checks
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IUnisonService _unisonService;
        private readonly IMonitoringService _monitoringService;
        private readonly ILogger<HealthController> _logger;

        /// <summary>
        /// Initializes a new instance of the HealthController
        /// </summary>
        /// <param name="unisonService">Service for communicating with the SOAP backend</param>
        /// <param name="monitoringService">Service for monitoring and metrics collection</param>
        /// <param name="logger">Logger for tracking health check operations</param>
        public HealthController(
            IUnisonService unisonService,
            IMonitoringService monitoringService,
            ILogger<HealthController> logger)
        {
            _unisonService = unisonService;
            _monitoringService = monitoringService;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint - fast response for load balancers and monitoring
        /// </summary>
        /// <returns>Basic health status according to Issue #6 specification</returns>
        /// <response code="200">Service is healthy and operational</response>
        /// <remarks>
        /// This endpoint provides a basic health check that returns quickly (target: &lt;50ms).
        /// It does not perform dependency checks and is designed for load balancers.
        /// 
        /// Response format matches Issue #6 specification:
        /// 
        ///     {
        ///         "status": "healthy",
        ///         "timestamp": "2025-09-11T15:30:00Z",
        ///         "version": "1.0.0",
        ///         "uptime": "2d 14h 30m"
        ///     }
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<HealthCheckResponse> GetHealth()
        {
            var response = new HealthCheckResponse
            {
                Status = "healthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Uptime = GetUptime()
            };

            _logger.LogInformation("Basic health check performed - Status: {Status}", response.Status);
            return Ok(response);
        }

        /// <summary>
        /// Detailed health check endpoint - comprehensive system and dependency status
        /// </summary>
        /// <returns>Comprehensive health status with system metrics according to Issue #6</returns>
        /// <response code="200">Service is healthy with detailed metrics</response>
        /// <response code="503">Service is unhealthy or dependencies are failing</response>
        /// <remarks>
        /// This endpoint provides comprehensive health information including system metrics,
        /// dependency status, and performance indicators as specified in Issue #6.
        /// 
        /// Response includes:
        /// - Basic health information
        /// - System metrics (CPU, memory, disk)
        /// - Application metrics (request counts, response times)
        /// - Dependency status (database, SOAP service)
        /// - Performance indicators
        /// 
        /// Example response:
        /// 
        ///     {
        ///         "status": "healthy",
        ///         "timestamp": "2025-09-11T15:30:00Z",
        ///         "version": "1.0.0",
        ///         "uptime": "2d 14h 30m",
        ///         "systemMetrics": {
        ///             "cpuUsagePercent": 15.2,
        ///             "memoryUsageMb": 256.8,
        ///             "diskUsagePercent": 45.1
        ///         },
        ///         "applicationMetrics": {
        ///             "totalRequests": 15420,
        ///             "averageResponseTimeMs": 125.4,
        ///             "errorRate": 0.02
        ///         },
        ///         "dependencies": [
        ///             {
        ///                 "name": "SOAP Service",
        ///                 "status": "healthy",
        ///                 "responseTimeMs": 85
        ///             }
        ///         ]
        ///     }
        /// </remarks>
        [HttpGet("detailed")]
        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<object>> GetDetailedHealth()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Get comprehensive health data using monitoring service
                var systemMetrics = await _monitoringService.GetSystemMetricsAsync();
                var applicationMetrics = _monitoringService.GetApplicationMetrics();

                // Check all dependencies
                var dependencyChecks = new List<Task<DependencyStatus>>();

                // Check SOAP service dependency
                var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    dependencyChecks.Add(_monitoringService.CheckDependencyAsync("SOAP Service", "soap://unison-service"));
                }

                var dependencyStatuses = await Task.WhenAll(dependencyChecks);

                // Determine overall health status
                var isHealthy = dependencyStatuses.All(d => d.IsHealthy);
                var overallStatus = isHealthy ? "healthy" : "unhealthy";

                var response = new
                {
                    Status = overallStatus,
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Uptime = GetUptime(),
                    SystemMetrics = new
                    {
                        CpuUsagePercent = systemMetrics.CpuUsagePercent,
                        WorkingSetMemoryMB = systemMetrics.WorkingSetMemoryMB,
                        PrivateMemoryMB = systemMetrics.PrivateMemoryMB,
                        ThreadCount = systemMetrics.ThreadCount,
                        ProcessorCount = systemMetrics.ProcessorCount,
                        MachineName = systemMetrics.MachineName
                    },
                    ApplicationMetrics = new
                    {
                        TotalRequests = applicationMetrics.TotalRequests,
                        AverageResponseTimeMs = Math.Round(applicationMetrics.AverageResponseTimeMs, 2),
                        P95ResponseTimeMs = Math.Round(applicationMetrics.P95ResponseTimeMs, 2),
                        ErrorRate = Math.Round(applicationMetrics.ErrorRate, 4),
                        ActiveConnections = applicationMetrics.ActiveConnections
                    },
                    Dependencies = dependencyStatuses.Select(d => new
                    {
                        Name = d.Name,
                        Status = d.IsHealthy ? "healthy" : "unhealthy",
                        ResponseTimeMs = Math.Round(d.ResponseTimeMs, 2),
                        LastCheckTime = d.LastCheckTime,
                        ErrorMessage = d.ErrorMessage
                    }).ToList(),
                    Performance = new
                    {
                        HealthCheckDurationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2)
                    }
                };

                stopwatch.Stop();

                _logger.LogInformation("Detailed health check completed - Status: {Status}, Duration: {Duration}ms",
                    overallStatus, stopwatch.ElapsedMilliseconds);

                return isHealthy ? Ok(response) : StatusCode(503, response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Error performing detailed health check");

                var errorResponse = new
                {
                    Status = "unhealthy",
                    Timestamp = DateTime.UtcNow,
                    Version = "1.0.0",
                    Error = "Health check failed",
                    Performance = new
                    {
                        HealthCheckDurationMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2)
                    }
                };

                return StatusCode(503, errorResponse);
            }
        }

        /// <summary>
        /// Readiness check endpoint - indicates if service is ready to serve requests
        /// </summary>
        /// <returns>Readiness status according to Issue #6 specification</returns>
        /// <response code="200">Service is ready to serve requests</response>
        /// <response code="503">Service is not ready (still initializing or dependency unavailable)</response>
        /// <remarks>
        /// This endpoint checks if the service is ready to handle requests.
        /// Used by orchestrators like Kubernetes to determine when to route traffic.
        /// 
        /// Response format according to Issue #6:
        /// 
        ///     {
        ///         "status": "ready",
        ///         "timestamp": "2025-09-11T15:30:00Z",
        ///         "checks": {
        ///             "database": "ready",
        ///             "cache": "ready"
        ///         }
        ///     }
        /// </remarks>
        [HttpGet("ready")]
        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<object>> GetReadiness()
        {
            try
            {
                var checks = new Dictionary<string, string>();
                var allReady = true;

                // Check SOAP service readiness if token is provided
                var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var dependencyStatus = await _monitoringService.CheckDependencyAsync("SOAP Service", "soap://unison-service");
                        var status = dependencyStatus.IsHealthy ? "ready" : "not_ready";
                        checks.Add("soap_service", status);

                        if (!dependencyStatus.IsHealthy)
                        {
                            allReady = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        checks.Add("soap_service", "not_ready");
                        allReady = false;
                        _logger.LogWarning(ex, "SOAP service readiness check failed");
                    }
                }
                else
                {
                    checks.Add("soap_service", "skipped");
                }

                // Check application state
                checks.Add("application", "ready");

                var response = new
                {
                    Status = allReady ? "ready" : "not_ready",
                    Timestamp = DateTime.UtcNow,
                    Checks = checks
                };

                var statusCode = allReady ? 200 : 503;
                _logger.LogInformation("Readiness check completed - Status: {Status}", response.Status);

                return StatusCode(statusCode, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during readiness check");
                return StatusCode(503, new
                {
                    Status = "not_ready",
                    Error = "Readiness check failed",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Liveness check endpoint - indicates if the application process is alive
        /// </summary>
        /// <returns>Liveness status according to Issue #6 specification</returns>
        /// <response code="200">Service process is alive and running</response>
        /// <remarks>
        /// This endpoint performs a simple check to verify that the application process
        /// is alive and responding to requests. Used by orchestrators like Kubernetes
        /// to determine if a container needs to be restarted.
        /// 
        /// Response format according to Issue #6:
        /// 
        ///     {
        ///         "status": "alive",
        ///         "timestamp": "2025-09-11T15:30:00Z",
        ///         "processId": 1234,
        ///         "uptime": "2d 14h 30m"
        ///     }
        /// </remarks>
        [HttpGet("live")]
        [ProducesResponseType(200)]
        public ActionResult<object> GetLiveness()
        {
            var response = new
            {
                Status = "alive",
                Timestamp = DateTime.UtcNow,
                ProcessId = Environment.ProcessId,
                Uptime = GetUptime(),
                ThreadCount = Environment.CurrentManagedThreadId
            };

            _logger.LogDebug("Liveness check performed - ProcessId: {ProcessId}", Environment.ProcessId);
            return Ok(response);
        }

        /// <summary>
        /// Metrics endpoint - provides performance metrics and statistics
        /// </summary>
        /// <returns>Detailed performance metrics according to Issue #6</returns>
        /// <response code="200">Performance metrics and statistics</response>
        /// <remarks>
        /// This endpoint provides comprehensive performance metrics including:
        /// - Request statistics and response times
        /// - System resource utilization
        /// - Per-endpoint metrics
        /// - Cache performance (if applicable)
        /// 
        /// Response format according to Issue #6:
        /// 
        ///     {
        ///         "timestamp": "2025-09-11T15:30:00Z",
        ///         "uptime": "2d 14h 30m",
        ///         "requestMetrics": {
        ///             "totalRequests": 15420,
        ///             "successfulRequests": 15350,
        ///             "errorRequests": 70,
        ///             "errorRate": 0.45
        ///         },
        ///         "endpointMetrics": {...}
        ///     }
        /// </remarks>
        [HttpGet("metrics")]
        [ProducesResponseType(200)]
        public ActionResult<object> GetMetrics()
        {
            try
            {
                var requestMetrics = _monitoringService.GetRequestMetrics();
                var applicationMetrics = _monitoringService.GetApplicationMetrics();
                var cacheMetrics = _monitoringService.GetCacheMetrics();

                var response = new
                {
                    Timestamp = DateTime.UtcNow,
                    Uptime = GetUptime(),
                    RequestMetrics = new
                    {
                        TotalRequests = requestMetrics.TotalRequests,
                        SuccessfulRequests = requestMetrics.SuccessfulRequests,
                        ErrorRequests = requestMetrics.ErrorRequests,
                        ErrorRate = Math.Round(requestMetrics.ErrorRate, 4),
                        StatusCodeCounts = requestMetrics.StatusCodeCounts,
                        LastResetTime = requestMetrics.LastResetTime
                    },
                    ApplicationMetrics = new
                    {
                        TotalRequests = applicationMetrics.TotalRequests,
                        AverageResponseTimeMs = Math.Round(applicationMetrics.AverageResponseTimeMs, 2),
                        P95ResponseTimeMs = Math.Round(applicationMetrics.P95ResponseTimeMs, 2),
                        ErrorRate = Math.Round(applicationMetrics.ErrorRate, 4),
                        ActiveConnections = applicationMetrics.ActiveConnections,
                        LastResetTime = applicationMetrics.LastResetTime
                    },
                    EndpointMetrics = requestMetrics.EndpointMetrics.ToDictionary(
                        kvp => kvp.Key,
                        kvp => new
                        {
                            RequestCount = kvp.Value.RequestCount,
                            AverageResponseTimeMs = Math.Round(kvp.Value.AverageResponseTimeMs, 2),
                            MinResponseTimeMs = Math.Round(kvp.Value.MinResponseTimeMs, 2),
                            MaxResponseTimeMs = Math.Round(kvp.Value.MaxResponseTimeMs, 2),
                            ErrorCount = kvp.Value.ErrorCount,
                            ErrorRate = kvp.Value.RequestCount > 0 ? Math.Round((kvp.Value.ErrorCount / (double)kvp.Value.RequestCount) * 100, 4) : 0.0,
                            LastAccessTime = kvp.Value.LastAccessTime
                        }
                    ),
                    CacheMetrics = new
                    {
                        HitCount = cacheMetrics.HitCount,
                        MissCount = cacheMetrics.MissCount,
                        HitRatio = Math.Round(cacheMetrics.HitRatio, 4),
                        TotalEntries = cacheMetrics.TotalEntries,
                        EvictionCount = cacheMetrics.EvictionCount,
                        MemoryUsageBytes = cacheMetrics.MemoryUsageBytes,
                        LastResetTime = cacheMetrics.LastResetTime
                    }
                };

                _logger.LogDebug("Metrics retrieved successfully");
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metrics");
                return StatusCode(500, new
                {
                    Error = "Failed to retrieve metrics",
                    Timestamp = DateTime.UtcNow
                });
            }
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

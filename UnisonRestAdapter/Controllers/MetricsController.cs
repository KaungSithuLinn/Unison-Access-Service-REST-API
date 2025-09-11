using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace UnisonRestAdapter.Controllers
{
    /// <summary>
    /// REST API controller for service metrics
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly ILogger<MetricsController> _logger;

        /// <summary>
        /// Initializes a new instance of the MetricsController
        /// </summary>
        /// <param name="logger">Logger for tracking controller operations</param>
        public MetricsController(ILogger<MetricsController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Gets basic service metrics and statistics
        /// </summary>
        /// <returns>Service metrics including uptime, memory usage, and system information</returns>
        /// <response code="200">Metrics retrieved successfully</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/metrics
        /// 
        /// No authentication required for basic metrics.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "service": "UnisonRestAdapter",
        ///         "version": "1.0.0",
        ///         "uptime": "2.00:30:45",
        ///         "environment": "Development",
        ///         "system": {
        ///             "processId": 12345,
        ///             "workingSetMemory": 52428800,
        ///             "gcTotalMemory": 10485760,
        ///             "threadCount": 25
        ///         },
        ///         "timestamp": "2025-09-11T10:00:00Z"
        ///     }
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(200)]
        public ActionResult<object> GetMetrics()
        {
            _logger.LogInformation("Received GetMetrics request");

            try
            {
                using var process = Process.GetCurrentProcess();

                var metrics = new
                {
                    service = "UnisonRestAdapter",
                    version = "1.0.0",
                    uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    system = new
                    {
                        processId = process.Id,
                        workingSetMemory = process.WorkingSet64,
                        gcTotalMemory = GC.GetTotalMemory(false),
                        threadCount = process.Threads.Count
                    },
                    timestamp = DateTime.UtcNow
                };

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetMetrics request");
                var errorResponse = new
                {
                    service = "UnisonRestAdapter",
                    version = "1.0.0",
                    error = "Failed to retrieve metrics",
                    message = ex.Message,
                    timestamp = DateTime.UtcNow
                };
                return StatusCode(500, errorResponse);
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using UnisonRestAdapter.Services;

namespace UnisonRestAdapter.Controllers;

/// <summary>
/// Controller for performance monitoring and statistics
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceMonitoringService _performanceMonitoring;
    private readonly IResponseCacheService _cacheService;
    private readonly ILogger<PerformanceController> _logger;

    /// <summary>
    /// Initializes a new instance of the PerformanceController
    /// </summary>
    /// <param name="performanceMonitoring">Performance monitoring service</param>
    /// <param name="cacheService">Response cache service</param>
    /// <param name="logger">Logger instance</param>
    public PerformanceController(
        IPerformanceMonitoringService performanceMonitoring,
        IResponseCacheService cacheService,
        ILogger<PerformanceController> logger)
    {
        _performanceMonitoring = performanceMonitoring ?? throw new ArgumentNullException(nameof(performanceMonitoring));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets current performance statistics
    /// </summary>
    /// <returns>Performance statistics</returns>
    [HttpGet("statistics")]
    public ActionResult<PerformanceStatistics> GetStatistics()
    {
        try
        {
            var statistics = _performanceMonitoring.GetStatistics();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance statistics");
            return StatusCode(500, new { message = "Error retrieving performance statistics" });
        }
    }

    /// <summary>
    /// Gets system health status with performance metrics
    /// </summary>
    /// <returns>System health information</returns>
    [HttpGet("health")]
    public ActionResult<object> GetHealth()
    {
        try
        {
            var statistics = _performanceMonitoring.GetStatistics();
            var process = System.Diagnostics.Process.GetCurrentProcess();

            var health = new
            {
                Status = "Healthy",
                Timestamp = DateTimeOffset.UtcNow,
                Performance = statistics,
                System = new
                {
                    ProcessId = process.Id,
                    StartTime = process.StartTime,
                    Uptime = DateTime.UtcNow - process.StartTime,
                    WorkingSet = process.WorkingSet64,
                    PrivateMemory = process.PrivateMemorySize64,
                    ThreadCount = process.Threads.Count,
                    HandleCount = process.HandleCount
                },
                Environment = new
                {
                    MachineName = Environment.MachineName,
                    ProcessorCount = Environment.ProcessorCount,
                    OSVersion = Environment.OSVersion.ToString(),
                    Framework = Environment.Version.ToString()
                }
            };

            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system health");
            return StatusCode(500, new { message = "Error retrieving system health" });
        }
    }

    /// <summary>
    /// Clears the response cache
    /// </summary>
    /// <param name="pattern">Optional pattern to match for selective clearing</param>
    /// <returns>Cache clear result</returns>
    [HttpPost("cache/clear")]
    public async Task<ActionResult> ClearCache([FromQuery] string? pattern = null)
    {
        try
        {
            if (string.IsNullOrEmpty(pattern))
            {
                // Clear all cache entries by removing with wildcard pattern
                await _cacheService.RemoveByPatternAsync("");
                _logger.LogInformation("All cache entries cleared");
                return Ok(new { message = "All cache entries cleared" });
            }
            else
            {
                await _cacheService.RemoveByPatternAsync(pattern);
                _logger.LogInformation("Cache entries cleared for pattern: {Pattern}", pattern);
                return Ok(new { message = $"Cache entries cleared for pattern: {pattern}" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache with pattern: {Pattern}", pattern);
            return StatusCode(500, new { message = "Error clearing cache" });
        }
    }

    /// <summary>
    /// Forces garbage collection (use with caution)
    /// </summary>
    /// <returns>Memory usage before and after GC</returns>
    [HttpPost("gc")]
    public ActionResult<object> ForceGarbageCollection()
    {
        try
        {
            var memoryBefore = GC.GetTotalMemory(false);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var memoryAfter = GC.GetTotalMemory(false);
            var memoryFreed = memoryBefore - memoryAfter;

            _logger.LogInformation("Forced garbage collection completed. Memory freed: {MemoryFreed:N0} bytes", memoryFreed);

            return Ok(new
            {
                MemoryBefore = memoryBefore,
                MemoryAfter = memoryAfter,
                MemoryFreed = memoryFreed,
                Timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forced garbage collection");
            return StatusCode(500, new { message = "Error during garbage collection" });
        }
    }
}

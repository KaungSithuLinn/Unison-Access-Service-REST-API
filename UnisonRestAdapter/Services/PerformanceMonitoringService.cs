using System.Diagnostics;

namespace UnisonRestAdapter.Services;

/// <summary>
/// Service for monitoring and tracking performance metrics
/// </summary>
public interface IPerformanceMonitoringService
{
    /// <summary>
    /// Start timing an operation
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <returns>Stopwatch instance</returns>
    Stopwatch StartTiming(string operationName, string correlationId);

    /// <summary>
    /// Stop timing and log performance metrics
    /// </summary>
    /// <param name="stopwatch">Stopwatch instance</param>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    /// <param name="success">Whether the operation was successful</param>
    /// <param name="additionalMetrics">Additional metrics to log</param>
    void StopTiming(Stopwatch stopwatch, string operationName, string correlationId, bool success = true, Dictionary<string, object>? additionalMetrics = null);

    /// <summary>
    /// Log memory usage metrics
    /// </summary>
    /// <param name="operationName">Name of the operation</param>
    /// <param name="correlationId">Correlation ID for tracking</param>
    void LogMemoryUsage(string operationName, string correlationId);

    /// <summary>
    /// Get current performance statistics
    /// </summary>
    /// <returns>Performance statistics</returns>
    PerformanceStatistics GetStatistics();
}

/// <summary>
/// Performance statistics data
/// </summary>
public class PerformanceStatistics
{
    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Total number of requests processed
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Number of successful requests
    /// </summary>
    public long SuccessfulRequests { get; set; }

    /// <summary>
    /// Number of failed requests
    /// </summary>
    public long FailedRequests { get; set; }

    /// <summary>
    /// Current memory usage in bytes
    /// </summary>
    public long MemoryUsageBytes { get; set; }

    /// <summary>
    /// CPU usage percentage
    /// </summary>
    public double CpuUsagePercentage { get; set; }

    /// <summary>
    /// Success rate percentage
    /// </summary>
    public double SuccessRate => TotalRequests > 0 ? (SuccessfulRequests * 100.0) / TotalRequests : 0;

    /// <summary>
    /// Requests per second (approximate)
    /// </summary>
    public double RequestsPerSecond { get; set; }
}

/// <summary>
/// Implementation of performance monitoring service
/// </summary>
public class PerformanceMonitoringService : IPerformanceMonitoringService
{
    private readonly ILogger<PerformanceMonitoringService> _logger;
    private readonly Dictionary<string, List<long>> _operationTimes;
    private readonly Dictionary<string, long> _operationCounts;
    private readonly Dictionary<string, long> _successCounts;
    private readonly Dictionary<string, long> _failureCounts;
    private readonly object _lockObject = new();
    private readonly Stopwatch _uptimeStopwatch;

    /// <summary>
    /// Initializes a new instance of the PerformanceMonitoringService
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public PerformanceMonitoringService(ILogger<PerformanceMonitoringService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _operationTimes = new Dictionary<string, List<long>>();
        _operationCounts = new Dictionary<string, long>();
        _successCounts = new Dictionary<string, long>();
        _failureCounts = new Dictionary<string, long>();
        _uptimeStopwatch = Stopwatch.StartNew();
    }

    /// <inheritdoc />
    public Stopwatch StartTiming(string operationName, string correlationId)
    {
        if (string.IsNullOrEmpty(operationName))
            throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));

        _logger.LogDebug("Starting timing for operation: {OperationName}, CorrelationId: {CorrelationId}",
            operationName, correlationId);

        return Stopwatch.StartNew();
    }

    /// <inheritdoc />
    public void StopTiming(Stopwatch stopwatch, string operationName, string correlationId, bool success = true, Dictionary<string, object>? additionalMetrics = null)
    {
        if (stopwatch == null)
            throw new ArgumentNullException(nameof(stopwatch));

        if (string.IsNullOrEmpty(operationName))
            throw new ArgumentException("Operation name cannot be null or empty", nameof(operationName));

        stopwatch.Stop();
        var elapsedMs = stopwatch.ElapsedMilliseconds;

        lock (_lockObject)
        {
            // Update operation times
            if (!_operationTimes.ContainsKey(operationName))
            {
                _operationTimes[operationName] = new List<long>();
            }
            _operationTimes[operationName].Add(elapsedMs);

            // Keep only last 1000 measurements for memory efficiency
            if (_operationTimes[operationName].Count > 1000)
            {
                _operationTimes[operationName].RemoveAt(0);
            }

            // Update counts
            _operationCounts.TryGetValue(operationName, out var count);
            _operationCounts[operationName] = count + 1;

            if (success)
            {
                _successCounts.TryGetValue(operationName, out var successCount);
                _successCounts[operationName] = successCount + 1;
            }
            else
            {
                _failureCounts.TryGetValue(operationName, out var failureCount);
                _failureCounts[operationName] = failureCount + 1;
            }
        }

        // Log performance metrics
        var logLevel = success ? LogLevel.Information : LogLevel.Warning;
        var metricsData = new Dictionary<string, object>
        {
            ["OperationName"] = operationName,
            ["CorrelationId"] = correlationId,
            ["ElapsedMilliseconds"] = elapsedMs,
            ["Success"] = success,
            ["Timestamp"] = DateTimeOffset.UtcNow
        };

        if (additionalMetrics != null)
        {
            foreach (var metric in additionalMetrics)
            {
                metricsData[metric.Key] = metric.Value;
            }
        }

        _logger.Log(logLevel, "Operation completed: {OperationName} in {ElapsedMs}ms, Success: {Success}, CorrelationId: {CorrelationId} {@Metrics}",
            operationName, elapsedMs, success, correlationId, metricsData);

        // Log warning for slow operations (> 5 seconds)
        if (elapsedMs > 5000)
        {
            _logger.LogWarning("Slow operation detected: {OperationName} took {ElapsedMs}ms, CorrelationId: {CorrelationId}",
                operationName, elapsedMs, correlationId);
        }
    }

    /// <inheritdoc />
    public void LogMemoryUsage(string operationName, string correlationId)
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var memoryUsage = process.WorkingSet64;
            var privateMem = process.PrivateMemorySize64;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var managedMemory = GC.GetTotalMemory(false);

            _logger.LogInformation("Memory usage for {OperationName}: Working Set: {WorkingSet:N0} bytes, Private: {PrivateMemory:N0} bytes, Managed: {ManagedMemory:N0} bytes, CorrelationId: {CorrelationId}",
                operationName, memoryUsage, privateMem, managedMemory, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging memory usage for operation: {OperationName}, CorrelationId: {CorrelationId}",
                operationName, correlationId);
        }
    }

    /// <inheritdoc />
    public PerformanceStatistics GetStatistics()
    {
        lock (_lockObject)
        {
            var totalRequests = _operationCounts.Values.Sum();
            var totalSuccesses = _successCounts.Values.Sum();
            var totalFailures = _failureCounts.Values.Sum();

            var allTimes = _operationTimes.Values.SelectMany(times => times).ToList();
            var averageResponseTime = allTimes.Any() ? allTimes.Average() : 0;

            var uptimeSeconds = _uptimeStopwatch.Elapsed.TotalSeconds;
            var requestsPerSecond = uptimeSeconds > 0 ? totalRequests / uptimeSeconds : 0;

            var currentProcess = Process.GetCurrentProcess();

            return new PerformanceStatistics
            {
                AverageResponseTime = averageResponseTime,
                TotalRequests = totalRequests,
                SuccessfulRequests = totalSuccesses,
                FailedRequests = totalFailures,
                MemoryUsageBytes = currentProcess.WorkingSet64,
                CpuUsagePercentage = GetCpuUsage(),
                RequestsPerSecond = requestsPerSecond
            };
        }
    }

    private double GetCpuUsage()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            return process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount / Environment.TickCount * 100;
        }
        catch
        {
            return 0; // Return 0 if unable to calculate CPU usage
        }
    }
}

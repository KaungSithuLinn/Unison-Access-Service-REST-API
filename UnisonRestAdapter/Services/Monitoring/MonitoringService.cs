using System.Collections.Concurrent;
using System.Diagnostics;
using UnisonRestAdapter.Configuration;
using UnisonRestAdapter.Models.Monitoring;
using Microsoft.Extensions.Options;

namespace UnisonRestAdapter.Services.Monitoring
{
    /// <summary>
    /// Implementation of monitoring service for metrics collection and health monitoring
    /// </summary>
    public class MonitoringService : IMonitoringService
    {
        private readonly ILogger<MonitoringService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly UnisonSettings _unisonSettings;

        // Thread-safe collections for metrics
        private readonly ConcurrentDictionary<string, EndpointMetrics> _endpointMetrics = new();
        private readonly ConcurrentDictionary<int, long> _statusCodeCounts = new();
        private readonly object _metricsLock = new();

        // Counters for metrics
        private long _totalRequests = 0;
        private long _successfulRequests = 0;
        private long _errorRequests = 0;
        private double _totalResponseTime = 0;
        private readonly List<double> _responseTimes = new();
        private DateTime _lastResetTime = DateTime.UtcNow;

        /// <summary>
        /// Initializes a new instance of the MonitoringService
        /// </summary>
        /// <param name="logger">Logger for the monitoring service</param>
        /// <param name="httpClientFactory">HTTP client factory for dependency checks</param>
        /// <param name="unisonSettings">Unison configuration settings</param>
        public MonitoringService(
            ILogger<MonitoringService> logger,
            IHttpClientFactory httpClientFactory,
            IOptions<UnisonSettings> unisonSettings)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _unisonSettings = unisonSettings.Value;
        }

        /// <summary>
        /// Gets comprehensive system metrics
        /// </summary>
        public async Task<SystemMetrics> GetSystemMetricsAsync()
        {
            var process = Process.GetCurrentProcess();

            return await Task.FromResult(new SystemMetrics
            {
                WorkingSetMemoryMB = process.WorkingSet64 / (1024 * 1024),
                PrivateMemoryMB = process.PrivateMemorySize64 / (1024 * 1024),
                ThreadCount = process.Threads.Count,
                Uptime = DateTime.UtcNow - process.StartTime.ToUniversalTime(),
                CpuUsagePercent = await GetCpuUsageAsync(),
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Gets application performance metrics
        /// </summary>
        public ApplicationMetrics GetApplicationMetrics()
        {
            lock (_metricsLock)
            {
                var errorRate = _totalRequests > 0 ? (_errorRequests / (double)_totalRequests) * 100 : 0;
                var avgResponseTime = _totalRequests > 0 ? _totalResponseTime / _totalRequests : 0;
                var p95ResponseTime = Calculate95thPercentile();

                return new ApplicationMetrics
                {
                    TotalRequests = _totalRequests,
                    AverageResponseTimeMs = Math.Round(avgResponseTime, 2),
                    P95ResponseTimeMs = Math.Round(p95ResponseTime, 2),
                    ErrorRate = Math.Round(errorRate, 2),
                    ActiveConnections = GetActiveConnectionCount(),
                    LastResetTime = _lastResetTime,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Records a request metric
        /// </summary>
        public void RecordRequestMetric(string endpoint, int statusCode, double responseTimeMs)
        {
            lock (_metricsLock)
            {
                _totalRequests++;
                _totalResponseTime += responseTimeMs;
                _responseTimes.Add(responseTimeMs);

                if (statusCode >= 200 && statusCode < 400)
                {
                    _successfulRequests++;
                }
                else
                {
                    _errorRequests++;
                }

                // Update status code counts
                _statusCodeCounts.AddOrUpdate(statusCode, 1, (key, value) => value + 1);

                // Update endpoint metrics
                _endpointMetrics.AddOrUpdate(endpoint,
                    new EndpointMetrics
                    {
                        Endpoint = endpoint,
                        RequestCount = 1,
                        AverageResponseTimeMs = responseTimeMs,
                        MinResponseTimeMs = responseTimeMs,
                        MaxResponseTimeMs = responseTimeMs,
                        ErrorCount = statusCode >= 400 ? 1 : 0,
                        LastAccessTime = DateTime.UtcNow
                    },
                    (key, existing) =>
                    {
                        var totalTime = existing.AverageResponseTimeMs * existing.RequestCount + responseTimeMs;
                        existing.RequestCount++;
                        existing.AverageResponseTimeMs = totalTime / existing.RequestCount;
                        existing.MinResponseTimeMs = Math.Min(existing.MinResponseTimeMs, responseTimeMs);
                        existing.MaxResponseTimeMs = Math.Max(existing.MaxResponseTimeMs, responseTimeMs);

                        if (statusCode >= 400)
                        {
                            existing.ErrorCount++;
                        }

                        existing.LastAccessTime = DateTime.UtcNow;
                        return existing;
                    });

                // Keep response times list manageable (keep only last 1000 entries)
                if (_responseTimes.Count > 1000)
                {
                    _responseTimes.RemoveRange(0, _responseTimes.Count - 1000);
                }
            }
        }

        /// <summary>
        /// Gets request metrics summary
        /// </summary>
        public RequestMetrics GetRequestMetrics()
        {
            lock (_metricsLock)
            {
                return new RequestMetrics
                {
                    TotalRequests = _totalRequests,
                    SuccessfulRequests = _successfulRequests,
                    ErrorRequests = _errorRequests,
                    EndpointMetrics = new Dictionary<string, EndpointMetrics>(_endpointMetrics),
                    StatusCodeCounts = new Dictionary<int, long>(_statusCodeCounts),
                    LastResetTime = _lastResetTime
                };
            }
        }

        /// <summary>
        /// Gets cache metrics if caching is enabled
        /// </summary>
        public CacheMetrics GetCacheMetrics()
        {
            // Basic implementation - would need to integrate with actual cache provider
            return new CacheMetrics
            {
                HitCount = 0,
                MissCount = 0,
                EvictionCount = 0,
                TotalEntries = 0,
                MemoryUsageBytes = 0,
                LastResetTime = _lastResetTime
            };
        }

        /// <summary>
        /// Checks if a dependency is healthy
        /// </summary>
        public async Task<DependencyStatus> CheckDependencyAsync(string dependencyName, string url)
        {
            var stopwatch = Stopwatch.StartNew();
            var status = new DependencyStatus
            {
                Name = dependencyName,
                Url = url,
                LastCheckTime = DateTime.UtcNow
            };

            try
            {
                using var httpClient = _httpClientFactory.CreateClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout for health checks

                var response = await httpClient.GetAsync(url);
                stopwatch.Stop();

                status.ResponseTimeMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                status.IsHealthy = response.IsSuccessStatusCode;

                if (!status.IsHealthy)
                {
                    status.ErrorMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
                }

                _logger.LogDebug("Dependency check for {DependencyName}: {Status} in {ResponseTime}ms",
                    dependencyName, status.Status, status.ResponseTimeMs);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                status.ResponseTimeMs = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2);
                status.IsHealthy = false;
                status.ErrorMessage = ex.Message;

                _logger.LogWarning(ex, "Dependency check failed for {DependencyName}: {Error}",
                    dependencyName, ex.Message);
            }

            return status;
        }

        /// <summary>
        /// Calculate 95th percentile response time
        /// </summary>
        private double Calculate95thPercentile()
        {
            if (_responseTimes.Count == 0) return 0;

            var sortedTimes = _responseTimes.OrderBy(x => x).ToArray();
            var percentileIndex = (int)Math.Ceiling(sortedTimes.Length * 0.95) - 1;
            percentileIndex = Math.Max(0, Math.Min(percentileIndex, sortedTimes.Length - 1));

            return sortedTimes[percentileIndex];
        }

        /// <summary>
        /// Get current CPU usage (simplified implementation)
        /// </summary>
        private async Task<double> GetCpuUsageAsync()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                var startTime = DateTime.UtcNow;
                var startCpuUsage = process.TotalProcessorTime;

                await Task.Delay(500); // Sample over 500ms

                var endTime = DateTime.UtcNow;
                var endCpuUsage = process.TotalProcessorTime;

                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);

                return Math.Round(cpuUsageTotal * 100, 2);
            }
            catch
            {
                return 0; // Return 0 if we can't calculate CPU usage
            }
        }

        /// <summary>
        /// Get active connection count (simplified implementation)
        /// </summary>
        private int GetActiveConnectionCount()
        {
            // This would need to be implemented based on the actual connection pooling mechanism
            // For now, return a placeholder value
            return 0;
        }
    }
}

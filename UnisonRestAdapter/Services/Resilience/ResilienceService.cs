using Polly;
using Polly.Extensions.Http;
using Polly.CircuitBreaker;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net;

namespace UnisonRestAdapter.Services.Resilience
{
    /// <summary>
    /// Service for managing resilience policies (retry, circuit breaker, timeout)
    /// </summary>
    public interface IResilienceService
    {
        /// <summary>
        /// Execute an HTTP operation with resilience policies
        /// </summary>
        Task<HttpResponseMessage> ExecuteHttpAsync(Func<Task<HttpResponseMessage>> operation, string operationName = "HTTP");

        /// <summary>
        /// Execute a generic operation with retry policy
        /// </summary>
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName = "Operation");

        /// <summary>
        /// Get current circuit breaker state
        /// </summary>
        CircuitState GetCircuitBreakerState();
    }

    /// <summary>
    /// Implementation of resilience service using Polly
    /// </summary>
    public class ResilienceService : IResilienceService
    {
        private readonly IAsyncPolicy<HttpResponseMessage> _httpRetryPolicy;
        private readonly IAsyncPolicy _retryPolicy;
        private readonly ICircuitBreakerPolicy _circuitBreakerPolicy;
        private readonly ILogger<ResilienceService> _logger;
        private readonly ResilienceSettings _settings;

        public ResilienceService(
            IOptions<ResilienceSettings> settings,
            ILogger<ResilienceService> logger)
        {
            _settings = settings.Value;
            _logger = logger;

            // Create circuit breaker policy
            _circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<TimeoutException>()
                .CircuitBreakerAsync(
                    exceptionsAllowedBeforeBreaking: _settings.CircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromSeconds(_settings.CircuitBreaker.DurationOfBreakSeconds),
                    onBreak: OnCircuitBreakerOpen,
                    onReset: OnCircuitBreakerReset,
                    onHalfOpen: OnCircuitBreakerHalfOpen);

            // Store individual policies for use
            _httpRetryPolicy = CreateHttpRetryPolicy();

            // Create generic retry policy
            _retryPolicy = Policy
                .Handle<Exception>(ex => ShouldRetryException(ex))
                .WaitAndRetryAsync(
                    retryCount: _settings.Retry.MaxAttempts,
                    sleepDurationProvider: CalculateRetryDelay,
                    onRetry: OnRetry);
        }

        public async Task<HttpResponseMessage> ExecuteHttpAsync(Func<Task<HttpResponseMessage>> operation, string operationName = "HTTP")
        {
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                _logger.LogInformation(
                    "Executing HTTP operation {OperationName} with resilience policies. CorrelationId: {CorrelationId}",
                    operationName, correlationId);

                var result = await ((IAsyncPolicy)_circuitBreakerPolicy).ExecuteAsync(async () =>
                {
                    return await _httpRetryPolicy.ExecuteAsync(async () =>
                    {
                        var response = await operation();

                        // Log response for monitoring
                        _logger.LogDebug(
                            "HTTP operation {OperationName} completed. Status: {StatusCode}, CorrelationId: {CorrelationId}",
                            operationName, response.StatusCode, correlationId);

                        return response;
                    });
                });

                return result;
            }
            catch (BrokenCircuitException ex)
            {
                _logger.LogWarning(
                    "Circuit breaker is open for operation {OperationName}. CorrelationId: {CorrelationId}",
                    operationName, correlationId);
                throw new ServiceUnavailableException($"Service is temporarily unavailable: {operationName}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "HTTP operation {OperationName} failed after all resilience attempts. CorrelationId: {CorrelationId}",
                    operationName, correlationId);
                throw;
            }
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, string operationName = "Operation")
        {
            var correlationId = Guid.NewGuid().ToString("N")[..8];

            try
            {
                _logger.LogInformation(
                    "Executing operation {OperationName} with retry policy. CorrelationId: {CorrelationId}",
                    operationName, correlationId);

                return await _retryPolicy.ExecuteAsync(operation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Operation {OperationName} failed after all retry attempts. CorrelationId: {CorrelationId}",
                    operationName, correlationId);
                throw;
            }
        }

        public CircuitState GetCircuitBreakerState()
        {
            return _circuitBreakerPolicy.CircuitState;
        }

        private IAsyncPolicy<HttpResponseMessage> CreateHttpRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException and 5XX, 408 status codes
                .OrResult(msg => _settings.Retry.RetriableStatusCodes.Contains((int)msg.StatusCode))
                .WaitAndRetryAsync(
                    retryCount: _settings.Retry.MaxAttempts,
                    sleepDurationProvider: CalculateRetryDelay,
                    onRetry: OnHttpRetry);
        }

        private TimeSpan CalculateRetryDelay(int retryAttempt)
        {
            var delay = TimeSpan.FromSeconds(_settings.Retry.BaseDelaySeconds * Math.Pow(2, retryAttempt - 1));
            var maxDelay = TimeSpan.FromSeconds(_settings.Retry.MaxDelaySeconds);

            if (delay > maxDelay)
                delay = maxDelay;

            // Add jitter to prevent thundering herd
            if (_settings.Retry.UseJitter)
            {
                var random = new Random();
                var jitter = TimeSpan.FromMilliseconds(random.Next(0, (int)delay.TotalMilliseconds / 2));
                delay = delay.Add(jitter);
            }

            return delay;
        }

        private bool ShouldRetryException(Exception exception)
        {
            return exception is HttpRequestException ||
                   exception is TaskCanceledException ||
                   exception is TimeoutException ||
                   exception is InvalidOperationException;
        }

        private void OnHttpRetry(DelegateResult<HttpResponseMessage> outcome, TimeSpan delay, int retryCount, Context context)
        {
            if (outcome.Exception != null)
            {
                _logger.LogWarning(
                    "HTTP retry {RetryCount} scheduled in {Delay}ms due to exception: {Exception}",
                    retryCount, delay.TotalMilliseconds, outcome.Exception.Message);
            }
            else
            {
                _logger.LogWarning(
                    "HTTP retry {RetryCount} scheduled in {Delay}ms due to status code: {StatusCode}",
                    retryCount, delay.TotalMilliseconds, outcome.Result?.StatusCode);
            }
        }

        private void OnRetry(Exception exception, TimeSpan delay, int retryCount, Context context)
        {
            _logger.LogWarning(
                "Retry {RetryCount} scheduled in {Delay}ms due to exception: {Exception}",
                retryCount, delay.TotalMilliseconds, exception.Message);
        }

        private void OnCircuitBreakerOpen(Exception exception, TimeSpan duration)
        {
            _logger.LogError(exception,
                "Circuit breaker opened for {Duration}s due to exception: {Exception}",
                duration.TotalSeconds, exception.Message);
        }

        private void OnCircuitBreakerReset()
        {
            _logger.LogInformation("Circuit breaker reset - service is healthy again");
        }

        private void OnCircuitBreakerHalfOpen()
        {
            _logger.LogInformation("Circuit breaker is half-open - testing service health");
        }
    }

    /// <summary>
    /// Exception thrown when service is unavailable due to circuit breaker
    /// </summary>
    public class ServiceUnavailableException : Exception
    {
        public ServiceUnavailableException(string message) : base(message) { }
        public ServiceUnavailableException(string message, Exception innerException) : base(message, innerException) { }
    }
}

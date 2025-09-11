using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace UnisonRestAdapter.Services;

/// <summary>
/// Service for caching API responses to improve performance
/// </summary>
public interface IResponseCacheService
{
    /// <summary>
    /// Get cached response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="cacheKey">Cache key</param>
    /// <returns>Cached response or null if not found</returns>
    Task<T?> GetAsync<T>(string cacheKey) where T : class;

    /// <summary>
    /// Set cached response
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="cacheKey">Cache key</param>
    /// <param name="value">Value to cache</param>
    /// <param name="expiration">Cache expiration time</param>
    Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration) where T : class;

    /// <summary>
    /// Remove cached response
    /// </summary>
    /// <param name="cacheKey">Cache key</param>
    Task RemoveAsync(string cacheKey);

    /// <summary>
    /// Remove all cached responses matching pattern
    /// </summary>
    /// <param name="pattern">Pattern to match</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Generate cache key for user operations
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="operation">Operation name</param>
    /// <returns>Cache key</returns>
    string GenerateUserCacheKey(string userId, string operation);

    /// <summary>
    /// Generate cache key for health check operations
    /// </summary>
    /// <param name="correlationId">Correlation ID</param>
    /// <returns>Cache key</returns>
    string GenerateHealthCacheKey(string correlationId);
}

/// <summary>
/// Implementation of response cache service using in-memory caching
/// </summary>
public class ResponseCacheService : IResponseCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<ResponseCacheService> _logger;
    private readonly HashSet<string> _cacheKeys;
    private readonly object _lockObject = new();

    /// <summary>
    /// Initializes a new instance of the ResponseCacheService
    /// </summary>
    /// <param name="memoryCache">Memory cache instance</param>
    /// <param name="logger">Logger instance</param>
    public ResponseCacheService(IMemoryCache memoryCache, ILogger<ResponseCacheService> logger)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheKeys = new HashSet<string>();
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string cacheKey) where T : class
    {
        try
        {
            if (string.IsNullOrEmpty(cacheKey))
                return Task.FromResult<T?>(null);

            if (_memoryCache.TryGetValue(cacheKey, out var cachedValue))
            {
                _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);

                if (cachedValue is string jsonString)
                {
                    return Task.FromResult(JsonSerializer.Deserialize<T>(jsonString));
                }

                return Task.FromResult(cachedValue as T);
            }

            _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);
            return Task.FromResult<T?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache for key: {CacheKey}", cacheKey);
            return Task.FromResult<T?>(null);
        }
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string cacheKey, T value, TimeSpan expiration) where T : class
    {
        try
        {
            if (string.IsNullOrEmpty(cacheKey) || value == null)
                return Task.CompletedTask;

            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration,
                SlidingExpiration = TimeSpan.FromMinutes(5), // Sliding window of 5 minutes
                Priority = CacheItemPriority.Normal,
                PostEvictionCallbacks =
                {
                    new PostEvictionCallbackRegistration
                    {
                        EvictionCallback = OnCacheEntryEvicted,
                        State = cacheKey
                    }
                }
            };

            // Serialize complex objects to JSON for better memory management
            object cacheValue = typeof(T) == typeof(string)
                ? (object)value
                : JsonSerializer.Serialize(value);

            _memoryCache.Set(cacheKey, cacheValue, options);

            lock (_lockObject)
            {
                _cacheKeys.Add(cacheKey);
            }

            _logger.LogDebug("Cached value for key: {CacheKey}, Expiration: {Expiration}", cacheKey, expiration);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {CacheKey}", cacheKey);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public Task RemoveAsync(string cacheKey)
    {
        try
        {
            if (string.IsNullOrEmpty(cacheKey))
                return Task.CompletedTask;

            _memoryCache.Remove(cacheKey);

            lock (_lockObject)
            {
                _cacheKeys.Remove(cacheKey);
            }

            _logger.LogDebug("Removed cache entry for key: {CacheKey}", cacheKey);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry for key: {CacheKey}", cacheKey);
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc />
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            if (string.IsNullOrEmpty(pattern))
                return;

            HashSet<string> keysToRemove;
            lock (_lockObject)
            {
                keysToRemove = _cacheKeys.Where(key => key.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToHashSet();
            }

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }

            _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
        }
    }

    /// <inheritdoc />
    public string GenerateUserCacheKey(string userId, string operation)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(operation))
            throw new ArgumentException("UserId and operation cannot be null or empty");

        return $"user:{userId.ToLowerInvariant()}:{operation.ToLowerInvariant()}";
    }

    /// <inheritdoc />
    public string GenerateHealthCacheKey(string correlationId)
    {
        if (string.IsNullOrEmpty(correlationId))
            throw new ArgumentException("CorrelationId cannot be null or empty");

        return $"health:{correlationId.ToLowerInvariant()}";
    }

    private void OnCacheEntryEvicted(object key, object? value, EvictionReason reason, object? state)
    {
        var cacheKey = state as string;
        if (!string.IsNullOrEmpty(cacheKey))
        {
            lock (_lockObject)
            {
                _cacheKeys.Remove(cacheKey);
            }

            _logger.LogDebug("Cache entry evicted for key: {CacheKey}, Reason: {Reason}", cacheKey, reason);
        }
    }
}

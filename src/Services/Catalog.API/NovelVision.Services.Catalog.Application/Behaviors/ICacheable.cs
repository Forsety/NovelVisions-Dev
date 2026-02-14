using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace NovelVision.Services.Catalog.Application.Behaviors;

public interface ICacheable
{
    string CacheKey { get; }
    TimeSpan? SlidingExpiration { get; }
    TimeSpan? AbsoluteExpiration { get; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheable
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        IDistributedCache cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;

        // Try to get from cache
        var cachedValue = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedValue))
        {
            _logger.LogDebug("Cache hit for key: {CacheKey}", cacheKey);
            return JsonSerializer.Deserialize<TResponse>(cachedValue)!;
        }

        _logger.LogDebug("Cache miss for key: {CacheKey}", cacheKey);

        // Execute the request
        var response = await next();

        // Cache the response
        var options = new DistributedCacheEntryOptions();

        if (request.SlidingExpiration.HasValue)
        {
            options.SlidingExpiration = request.SlidingExpiration.Value;
        }

        if (request.AbsoluteExpiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = request.AbsoluteExpiration.Value;
        }

        await _cache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(response),
            options,
            cancellationToken);

        return response;
    }
}

using Microsoft.Extensions.Caching.Memory;

namespace StayHealthy.Application.Caching;

public class CacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public CacheProvider(IMemoryCache cache)
    {
        _cache = cache;
    }
    
    public T GetOrAdd<T>(string cacheKey, Func<T> getItemCallback, TimeSpan expiration)
    {
        if (!_cache.TryGetValue(cacheKey, out T? cacheEntry))
        {
            cacheEntry = SetCache(cacheKey, getItemCallback, expiration);
        }
        
        if (cacheEntry is null)
        {
            Remove(cacheKey);
            cacheEntry = SetCache(cacheKey, getItemCallback, expiration);
        }
        
        return cacheEntry;
    }
    
    public void Remove(string cacheKey)
    {
        _cache.Remove(cacheKey);
    }
    
    private T SetCache<T>(string cacheKey, Func<T> getItemCallback, TimeSpan expiration)
    {
        var cacheEntry = getItemCallback();
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(expiration);

        _cache.Set(cacheKey, cacheEntry, cacheEntryOptions);

        return cacheEntry;
    }
}
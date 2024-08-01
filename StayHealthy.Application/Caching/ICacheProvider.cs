namespace StayHealthy.Application.Caching;

public interface ICacheProvider
{
    T GetOrAdd<T>(string cacheKey, Func<T> getItemCallback, TimeSpan expiration);
    void Remove(string cacheKey);
}
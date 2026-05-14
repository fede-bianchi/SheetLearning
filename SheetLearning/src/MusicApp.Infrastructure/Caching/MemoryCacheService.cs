using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using MusicApp.Application.Caching;

namespace MusicApp.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ConcurrentDictionary<string, bool> _keys = new();

    public MemoryCacheService(IMemoryCache cache) => _cache = cache;

    public async Task<T> GetOrCreateAsync<T>(
        string cacheKey, Func<Task<T>> factory, TimeSpan duration)
    {
        if (_cache.TryGetValue(cacheKey, out T? cached))
            return cached!;

        var value = await factory();
        _cache.Set(cacheKey, value, duration);
        _keys.TryAdd(cacheKey, true);
        return value;
    }

    public void Invalidate(string cacheKey)
    {
        _cache.Remove(cacheKey);
        _keys.TryRemove(cacheKey, out _);
    }

    public void InvalidateByPrefix(string prefix)
    {
        foreach (var key in _keys.Keys.Where(k => k.StartsWith(prefix)))
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
    }
}

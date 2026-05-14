namespace MusicApp.Application.Caching;

public interface ICacheService
{
    Task<T> GetOrCreateAsync<T>(
        string cacheKey,
        Func<Task<T>> factory,
        TimeSpan duration);

    void Invalidate(string cacheKey);
    void InvalidateByPrefix(string prefix);
}

using MusicApp.Application.Caching;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Bundles;

public class GetActiveBundlesHandler
{
    private readonly ILessonBundleRepository _bundleRepository;
    private readonly ICacheService _cache;

    public GetActiveBundlesHandler(
        ILessonBundleRepository bundleRepository,
        ICacheService cache)
    {
        _bundleRepository = bundleRepository;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<LessonBundleDto>>> HandleAsync()
    {
        var dtos = await _cache.GetOrCreateAsync(
            CacheKeys.ActiveBundles,
            async () =>
            {
                var bundles = await _bundleRepository.GetActiveAsync();
                return bundles.Select(b => new LessonBundleDto(
                    b.Id, b.Nome, b.NumeroLezioni, b.Prezzo,
                    b.ScontoPercentuale, b.IsActive, b.ExpiresAfterDays
                )).ToList() as IReadOnlyList<LessonBundleDto>;
            },
            TimeSpan.FromMinutes(5));

        return Result<IReadOnlyList<LessonBundleDto>>.Ok(dtos);
    }
}

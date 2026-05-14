using MusicApp.Application.Caching;
using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class ToggleBundleHandler
{
    private readonly ILessonBundleRepository _bundleRepository;
    private readonly ICacheService _cache;

    public ToggleBundleHandler(
        ILessonBundleRepository bundleRepository,
        ICacheService cache)
    {
        _bundleRepository = bundleRepository;
        _cache = cache;
    }

    public async Task<Result<bool>> HandleAsync(int bundleId)
    {
        var bundle = await _bundleRepository.GetByIdAsync(bundleId);
        if (bundle is null)
            return Result<bool>.Fail(ErrorCodes.BundleNotFound, "Bundle non trovato.");

        bundle.IsActive = !bundle.IsActive;
        await _bundleRepository.UpdateAsync(bundle);

        _cache.Invalidate(CacheKeys.ActiveBundles);

        return Result<bool>.Ok(true);
    }
}

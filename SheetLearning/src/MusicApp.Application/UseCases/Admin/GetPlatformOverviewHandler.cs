using MusicApp.Application.Caching;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetPlatformOverviewHandler
{
    private readonly IAdminStatsRepository _adminStatsRepository;
    private readonly ICacheService _cache;

    public GetPlatformOverviewHandler(
        IAdminStatsRepository adminStatsRepository,
        ICacheService cache)
    {
        _adminStatsRepository = adminStatsRepository;
        _cache = cache;
    }

    public async Task<Result<PlatformStatsDto>> HandleAsync()
    {
        var dto = await _cache.GetOrCreateAsync(
            CacheKeys.PlatformStats,
            () => _adminStatsRepository.GetPlatformOverviewAsync(),
            TimeSpan.FromMinutes(5));

        return Result<PlatformStatsDto>.Ok(dto);
    }
}

using MusicApp.Application.DTOs;

namespace MusicApp.Application.Interfaces;

public interface IAdminStatsRepository
{
    Task<PlatformStatsDto> GetPlatformOverviewAsync();
    Task<RevenueStatsDto> GetRevenueAsync(DateOnly dal, DateOnly al);

    Task<IReadOnlyList<LevelCompletionStatsDto>> GetLevelCompletionStatsAsync();
}

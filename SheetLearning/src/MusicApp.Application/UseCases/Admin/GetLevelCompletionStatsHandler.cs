using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetLevelCompletionStatsHandler
{
    private readonly IAdminStatsRepository _statsRepo;

    public GetLevelCompletionStatsHandler(IAdminStatsRepository statsRepo)
    {
        _statsRepo = statsRepo;
    }

    public async Task<Result<IReadOnlyList<LevelCompletionStatsDto>>>
        HandleAsync()
    {
        var stats = await _statsRepo.GetLevelCompletionStatsAsync();
        return Result<IReadOnlyList<LevelCompletionStatsDto>>.Ok(stats);
    }
}

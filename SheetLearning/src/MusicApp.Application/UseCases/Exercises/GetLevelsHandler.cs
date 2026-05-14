using MusicApp.Application.Caching;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetLevelsHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IUserLevelProgressRepository _userLevelProgressRepository;
    private readonly ICacheService _cache;

    public GetLevelsHandler(
        ILevelRepository levelRepository,
        IUserLevelProgressRepository userLevelProgressRepository,
        ICacheService cache)
    {
        _levelRepository = levelRepository;
        _userLevelProgressRepository = userLevelProgressRepository;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<LevelWithProgressDto>>> HandleAsync(int userId)
    {
        var result = await _cache.GetOrCreateAsync(
            CacheKeys.UserLevels(userId),
            async () =>
            {
                var levels = await _levelRepository.GetAllAsync();
                var progressRows = await _userLevelProgressRepository.GetAllByUserAsync(userId);
                var progressByLevelId = progressRows.ToDictionary(progress => progress.LevelId);

                return levels
                    .OrderBy(level => level.ExerciseTypeId)
                    .ThenBy(level => level.NumeroLivello)
                    .Select(level =>
                    {
                        progressByLevelId.TryGetValue(level.Id, out var progress);
                        return level.ToLevelWithProgressDto(progress);
                    })
                    .ToList() as IReadOnlyList<LevelWithProgressDto>;
            },
            TimeSpan.FromMinutes(5));

        return Result<IReadOnlyList<LevelWithProgressDto>>.Ok(result);
    }
}

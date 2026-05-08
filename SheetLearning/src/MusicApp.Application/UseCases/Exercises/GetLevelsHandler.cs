using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetLevelsHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IUserLevelProgressRepository _userLevelProgressRepository;

    public GetLevelsHandler(
        ILevelRepository levelRepository,
        IUserLevelProgressRepository userLevelProgressRepository)
    {
        _levelRepository = levelRepository;
        _userLevelProgressRepository = userLevelProgressRepository;
    }

    public async Task<Result<IReadOnlyList<LevelWithProgressDto>>> HandleAsync(int userId)
    {
        var levels = await _levelRepository.GetAllAsync();
        var progressRows = await _userLevelProgressRepository.GetAllByUserAsync(userId);
        var progressByLevelId = progressRows.ToDictionary(progress => progress.LevelId);

        var result = levels
            .OrderBy(level => level.ExerciseTypeId)
            .ThenBy(level => level.NumeroLivello)
            .Select(level =>
            {
                progressByLevelId.TryGetValue(level.Id, out var progress);
                return level.ToLevelWithProgressDto(progress);
            })
            .ToList();

        return Result<IReadOnlyList<LevelWithProgressDto>>.Ok(result);
    }
}

using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetLevelDetailHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IUserLevelProgressRepository _userLevelProgressRepository;

    public GetLevelDetailHandler(
        ILevelRepository levelRepository,
        IUserLevelProgressRepository userLevelProgressRepository)
    {
        _levelRepository = levelRepository;
        _userLevelProgressRepository = userLevelProgressRepository;
    }

    public async Task<Result<LevelDetailDto>> HandleAsync(int userId, int levelId)
    {
        var level = await _levelRepository.GetByIdAsync(levelId);
        if (level is null)
        {
            return Result<LevelDetailDto>.Fail(
                ErrorCodes.LevelNotFound,
                "Livello non trovato.");
        }

        var progress = await _userLevelProgressRepository.GetAsync(userId, levelId);
        return Result<LevelDetailDto>.Ok(level.ToLevelDetailDto(progress));
    }
}

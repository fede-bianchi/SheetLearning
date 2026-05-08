using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetLevelsByExerciseTypeHandler
{
    private readonly ILevelRepository _levelRepository;
    private readonly IUserLevelProgressRepository _userLevelProgressRepository;
    private readonly IExerciseTypeRepository _exerciseTypeRepository;

    public GetLevelsByExerciseTypeHandler(
        ILevelRepository levelRepository,
        IUserLevelProgressRepository userLevelProgressRepository,
        IExerciseTypeRepository exerciseTypeRepository)
    {
        _levelRepository = levelRepository;
        _userLevelProgressRepository = userLevelProgressRepository;
        _exerciseTypeRepository = exerciseTypeRepository;
    }

    public async Task<Result<IReadOnlyList<LevelWithProgressDto>>> HandleAsync(int userId, int exerciseTypeId)
    {
        var exerciseType = await _exerciseTypeRepository.GetByIdAsync(exerciseTypeId);
        if (exerciseType is null)
        {
            return Result<IReadOnlyList<LevelWithProgressDto>>.Fail(
                ErrorCodes.ExerciseTypeNotFound,
                "Tipo esercizio non trovato.");
        }

        var levels = await _levelRepository.GetByExerciseTypeAsync(exerciseTypeId);
        var progressRows = await _userLevelProgressRepository.GetAllByUserAsync(userId);
        var progressByLevelId = progressRows.ToDictionary(progress => progress.LevelId);

        var result = levels
            .OrderBy(level => level.NumeroLivello)
            .Select(level =>
            {
                progressByLevelId.TryGetValue(level.Id, out var progress);
                return level.ToLevelWithProgressDto(progress);
            })
            .ToList();

        return Result<IReadOnlyList<LevelWithProgressDto>>.Ok(result);
    }
}

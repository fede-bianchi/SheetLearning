using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetMyProgressHandler
{
    private readonly IExerciseTypeRepository _exerciseTypeRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IUserLevelProgressRepository _userLevelProgressRepository;
    private readonly IBestScoreRepository _bestScoreRepository;

    public GetMyProgressHandler(
        IExerciseTypeRepository exerciseTypeRepository,
        ILevelRepository levelRepository,
        IUserLevelProgressRepository userLevelProgressRepository,
        IBestScoreRepository bestScoreRepository)
    {
        _exerciseTypeRepository = exerciseTypeRepository;
        _levelRepository = levelRepository;
        _userLevelProgressRepository = userLevelProgressRepository;
        _bestScoreRepository = bestScoreRepository;
    }

    public async Task<Result<UserProgressDto>> HandleAsync(int userId)
    {
        var exerciseTypes = await _exerciseTypeRepository.GetAllAsync();
        var levels = await _levelRepository.GetAllAsync();
        var progressRows = await _userLevelProgressRepository.GetAllByUserAsync(userId);
        var bestScores = await _bestScoreRepository.GetAllByUserAsync(userId);

        var progressByLevelId = progressRows.ToDictionary(progress => progress.LevelId);
        var bestScoreByTypeId = bestScores.ToDictionary(score => score.ExerciseTypeId);

        var exerciseProgresses = exerciseTypes
            .OrderBy(exerciseType => exerciseType.Id)
            .Select(exerciseType =>
            {
                bestScoreByTypeId.TryGetValue(exerciseType.Id, out var bestScore);

                var levelDtos = levels
                    .Where(level => level.ExerciseTypeId == exerciseType.Id)
                    .OrderBy(level => level.NumeroLivello)
                    .Select(level =>
                    {
                        progressByLevelId.TryGetValue(level.Id, out var progress);
                        return level.ToLevelWithProgressDto(progress);
                    })
                    .ToList();

                return new ExerciseProgressDto(
                    exerciseType.Id,
                    exerciseType.Nome,
                    bestScore?.PunteggioMigliore,
                    levelDtos);
            })
            .ToList();

        return Result<UserProgressDto>.Ok(new UserProgressDto(exerciseProgresses));
    }
}

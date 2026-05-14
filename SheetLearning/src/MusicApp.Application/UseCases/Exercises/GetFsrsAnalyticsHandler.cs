using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetFsrsAnalyticsHandler
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IExerciseTypeRepository _exerciseTypeRepository;

    public GetFsrsAnalyticsHandler(
        IAttemptRepository attemptRepository,
        IExerciseTypeRepository exerciseTypeRepository)
    {
        _attemptRepository = attemptRepository;
        _exerciseTypeRepository = exerciseTypeRepository;
    }

    public async Task<Result<IReadOnlyList<FsrsElementStatDto>>> HandleAsync(
        int userId, int exerciseTypeId)
    {
        var exerciseType = await _exerciseTypeRepository.GetByIdAsync(exerciseTypeId);
        if (exerciseType is null)
            return Result<IReadOnlyList<FsrsElementStatDto>>.Fail(
                ErrorCodes.ExerciseTypeNotFound, "Tipo esercizio non trovato.");

        var stats = await _attemptRepository.GetFsrsAnalyticsAsync(userId, exerciseTypeId);
        return Result<IReadOnlyList<FsrsElementStatDto>>.Ok(stats);
    }
}

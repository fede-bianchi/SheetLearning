using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetMyAttemptsHandler
{
    private readonly IAttemptRepository _attemptRepository;
    private readonly IExerciseTypeRepository _exerciseTypeRepository;

    public GetMyAttemptsHandler(
        IAttemptRepository attemptRepository,
        IExerciseTypeRepository exerciseTypeRepository)
    {
        _attemptRepository = attemptRepository;
        _exerciseTypeRepository = exerciseTypeRepository;
    }

    public async Task<Result<IReadOnlyList<AttemptDto>>> HandleAsync(int userId, AttemptQuery query)
    {
        var attempts = await _attemptRepository.GetByUserAndTypeAsync(userId, query.ExerciseTypeId);
        var exerciseTypes = await _exerciseTypeRepository.GetAllAsync();
        var nameByExerciseTypeId = exerciseTypes.ToDictionary(exerciseType => exerciseType.Id, exerciseType => exerciseType.Nome);

        var result = attempts
            .Select(attempt =>
            {
                var nomeEsercizio = nameByExerciseTypeId.GetValueOrDefault(attempt.ExerciseTypeId, "Sconosciuto");
                return attempt.ToAttemptDto(nomeEsercizio);
            })
            .ToList();

        return Result<IReadOnlyList<AttemptDto>>.Ok(result);
    }
}

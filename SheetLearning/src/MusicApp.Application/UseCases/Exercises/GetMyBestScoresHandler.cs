using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetMyBestScoresHandler
{
    private readonly IBestScoreRepository _bestScoreRepository;
    private readonly IExerciseTypeRepository _exerciseTypeRepository;

    public GetMyBestScoresHandler(
        IBestScoreRepository bestScoreRepository,
        IExerciseTypeRepository exerciseTypeRepository)
    {
        _bestScoreRepository = bestScoreRepository;
        _exerciseTypeRepository = exerciseTypeRepository;
    }

    public async Task<Result<IReadOnlyList<BestScoreDto>>> HandleAsync(int userId)
    {
        var bestScores = await _bestScoreRepository.GetAllByUserAsync(userId);
        var exerciseTypes = await _exerciseTypeRepository.GetAllAsync();
        var nameByExerciseTypeId = exerciseTypes.ToDictionary(exerciseType => exerciseType.Id, exerciseType => exerciseType.Nome);

        var result = bestScores
            .OrderBy(bestScore => bestScore.ExerciseTypeId)
            .Select(bestScore =>
            {
                var nomeEsercizio = nameByExerciseTypeId.GetValueOrDefault(bestScore.ExerciseTypeId, "Sconosciuto");
                return bestScore.ToBestScoreDto(nomeEsercizio);
            })
            .ToList();

        return Result<IReadOnlyList<BestScoreDto>>.Ok(result);
    }
}

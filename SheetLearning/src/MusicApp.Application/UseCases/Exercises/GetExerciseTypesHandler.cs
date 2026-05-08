using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetExerciseTypesHandler
{
    private readonly IExerciseTypeRepository _exerciseTypeRepository;

    public GetExerciseTypesHandler(IExerciseTypeRepository exerciseTypeRepository)
    {
        _exerciseTypeRepository = exerciseTypeRepository;
    }

    public async Task<Result<IReadOnlyList<ExerciseTypeDto>>> HandleAsync()
    {
        var exerciseTypes = await _exerciseTypeRepository.GetAllAsync();
        var result = exerciseTypes
            .Select(exerciseType => exerciseType.ToExerciseTypeDto())
            .ToList();

        return Result<IReadOnlyList<ExerciseTypeDto>>.Ok(result);
    }
}

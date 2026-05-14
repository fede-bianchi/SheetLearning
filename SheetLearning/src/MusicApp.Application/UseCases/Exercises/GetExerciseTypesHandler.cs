using MusicApp.Application.Caching;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Exercises;

public class GetExerciseTypesHandler
{
    private readonly IExerciseTypeRepository _exerciseTypeRepository;
    private readonly ICacheService _cache;

    public GetExerciseTypesHandler(
        IExerciseTypeRepository exerciseTypeRepository,
        ICacheService cache)
    {
        _exerciseTypeRepository = exerciseTypeRepository;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<ExerciseTypeDto>>> HandleAsync()
    {
        var result = await _cache.GetOrCreateAsync(
            CacheKeys.ExerciseTypes,
            async () =>
            {
                var types = await _exerciseTypeRepository.GetAllAsync();
                return types.Select(t => t.ToExerciseTypeDto()).ToList() as IReadOnlyList<ExerciseTypeDto>;
            },
            TimeSpan.FromHours(1));

        return Result<IReadOnlyList<ExerciseTypeDto>>.Ok(result);
    }
}

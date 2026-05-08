using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IExerciseTypeRepository
{
    Task<IReadOnlyList<ExerciseType>> GetAllAsync();
    Task<ExerciseType?> GetByIdAsync(int id);
}

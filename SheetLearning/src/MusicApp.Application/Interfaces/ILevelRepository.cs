using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ILevelRepository
{
    Task<IReadOnlyList<Level>> GetAllAsync();
    Task<Level?> GetByIdAsync(int id);
    Task<IReadOnlyList<Level>> GetByExerciseTypeAsync(int exerciseTypeId);
    Task<Level?> GetNextLevelAsync(int exerciseTypeId, byte currentNumeroLivello);
}

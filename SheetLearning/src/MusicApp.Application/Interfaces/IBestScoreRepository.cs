using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IBestScoreRepository
{
    Task<BestScore?> GetByUserAndTypeAsync(int userId, int exerciseTypeId);
    Task<IReadOnlyList<BestScore>> GetAllByUserAsync(int userId);
    Task<BestScore> CreateAsync(BestScore bestScore);
    Task<BestScore> UpdateAsync(BestScore bestScore);
}

using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IAttemptRepository
{
    Task<Attempt> CreateAsync(Attempt attempt);
    Task<IReadOnlyList<Attempt>> GetByUserAndTypeAsync(int userId, int? exerciseTypeId);
    Task<int> CountByUserAndTypeAsync(int userId, int exerciseTypeId);
    Task<IReadOnlyList<int>> GetOldestDeletableIdsAsync(
        int userId,
        int exerciseTypeId,
        IEnumerable<int> protectedAttemptIds,
        int excessCount);
    Task DeleteByIdsAsync(IEnumerable<int> ids);
}

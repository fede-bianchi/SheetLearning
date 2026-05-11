using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IVoteRepository
{
    Task<Vote?> GetByUserAndTargetAsync(int userId, string targetType, int targetId);

    Task<(int Upvotes, int Downvotes)> GetCountsAsync(string targetType, int targetId);

    Task<Dictionary<int, (int Upvotes, int Downvotes)>> GetCountsBatchAsync(
        string targetType,
        IEnumerable<int> targetIds);

    Task<Dictionary<int, string>> GetUserVotesBatchAsync(
        int userId,
        string targetType,
        IEnumerable<int> targetIds);

    Task<Vote> CreateAsync(Vote vote);
    Task<Vote> UpdateAsync(Vote vote);
    Task DeleteAsync(Vote vote);
}

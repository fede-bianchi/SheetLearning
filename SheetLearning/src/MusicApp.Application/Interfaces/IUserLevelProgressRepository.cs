using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IUserLevelProgressRepository
{
    Task<UserLevelProgress?> GetAsync(int userId, int levelId);
    Task<IReadOnlyList<UserLevelProgress>> GetAllByUserAsync(int userId);
    Task<UserLevelProgress> UpsertAsync(UserLevelProgress progress);
}

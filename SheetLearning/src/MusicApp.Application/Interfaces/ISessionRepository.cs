using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ISessionRepository
{
    Task<Session?> GetByTokenAsync(string token);
    Task<Session> CreateAsync(Session session);
    Task DeleteAsync(Session session);
    Task DeleteAllForUserAsync(int userId);
    Task DeleteExpiredAsync();
}

using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class SessionRepository : ISessionRepository
{
    private readonly AppDbContext _context;

    public SessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Session?> GetByTokenAsync(string token)
    {
        return _context.Sessions.FirstOrDefaultAsync(s => s.Token == token);
    }

    public async Task<Session> CreateAsync(Session session)
    {
        _context.Sessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task DeleteAsync(Session session)
    {
        _context.Sessions.Remove(session);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAllForUserAsync(int userId)
    {
        var sessions = await _context.Sessions.Where(s => s.UserId == userId).ToListAsync();
        if (sessions.Count == 0)
        {
            return;
        }

        _context.Sessions.RemoveRange(sessions);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredAsync()
    {
        var now = DateTime.UtcNow;
        var expiredSessions = await _context.Sessions.Where(s => s.ExpiresAt < now).ToListAsync();
        if (expiredSessions.Count == 0)
        {
            return;
        }

        _context.Sessions.RemoveRange(expiredSessions);
        await _context.SaveChangesAsync();
    }
}

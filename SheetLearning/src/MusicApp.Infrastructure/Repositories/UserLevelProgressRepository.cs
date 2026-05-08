using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class UserLevelProgressRepository : IUserLevelProgressRepository
{
    private readonly AppDbContext _context;

    public UserLevelProgressRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserLevelProgress?> GetAsync(int userId, int levelId)
    {
        return await _context.UserLevelProgress
            .FirstOrDefaultAsync(progress => progress.UserId == userId && progress.LevelId == levelId);
    }

    public async Task<IReadOnlyList<UserLevelProgress>> GetAllByUserAsync(int userId)
    {
        return await _context.UserLevelProgress
            .Where(progress => progress.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<UserLevelProgress> UpsertAsync(UserLevelProgress progress)
    {
        var existing = await _context.UserLevelProgress
            .FirstOrDefaultAsync(p => p.UserId == progress.UserId && p.LevelId == progress.LevelId);

        if (existing is null)
        {
            _context.UserLevelProgress.Add(progress);
            await _context.SaveChangesAsync();
            return progress;
        }

        existing.Sbloccato = progress.Sbloccato;
        existing.Completato = progress.Completato;
        existing.DataSblocco = progress.DataSblocco ?? existing.DataSblocco;
        existing.DataCompletamento = progress.DataCompletamento ?? existing.DataCompletamento;

        _context.UserLevelProgress.Update(existing);
        await _context.SaveChangesAsync();
        return existing;
    }
}

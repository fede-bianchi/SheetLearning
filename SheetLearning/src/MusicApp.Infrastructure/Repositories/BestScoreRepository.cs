using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class BestScoreRepository : IBestScoreRepository
{
    private readonly AppDbContext _context;

    public BestScoreRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BestScore?> GetByUserAndTypeAsync(int userId, int exerciseTypeId)
    {
        return await _context.BestScores
            .FirstOrDefaultAsync(bestScore =>
                bestScore.UserId == userId && bestScore.ExerciseTypeId == exerciseTypeId);
    }

    public async Task<IReadOnlyList<BestScore>> GetAllByUserAsync(int userId)
    {
        return await _context.BestScores
            .Where(bestScore => bestScore.UserId == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<BestScore> CreateAsync(BestScore bestScore)
    {
        _context.BestScores.Add(bestScore);
        await _context.SaveChangesAsync();
        return bestScore;
    }

    public async Task<BestScore> UpdateAsync(BestScore bestScore)
    {
        _context.BestScores.Update(bestScore);
        await _context.SaveChangesAsync();
        return bestScore;
    }
}

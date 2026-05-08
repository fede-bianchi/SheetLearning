using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class LevelRepository : ILevelRepository
{
    private readonly AppDbContext _context;

    public LevelRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Level>> GetAllAsync()
    {
        return await _context.Levels
            .AsNoTracking()
            .OrderBy(level => level.ExerciseTypeId)
            .ThenBy(level => level.NumeroLivello)
            .ToListAsync();
    }

    public async Task<Level?> GetByIdAsync(int id)
    {
        return await _context.Levels.FirstOrDefaultAsync(level => level.Id == id);
    }

    public async Task<IReadOnlyList<Level>> GetByExerciseTypeAsync(int exerciseTypeId)
    {
        return await _context.Levels
            .Where(level => level.ExerciseTypeId == exerciseTypeId)
            .OrderBy(level => level.NumeroLivello)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Level?> GetNextLevelAsync(int exerciseTypeId, byte currentNumeroLivello)
    {
        return await _context.Levels
            .Where(level => level.ExerciseTypeId == exerciseTypeId
                            && level.NumeroLivello == currentNumeroLivello + 1)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class LessonBundleRepository : ILessonBundleRepository
{
    private readonly AppDbContext _context;

    public LessonBundleRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LessonBundle>> GetActiveAsync()
    {
        return await _context.LessonBundles
            .Where(b => b.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LessonBundle?> GetByIdAsync(int id)
    {
        return await _context.LessonBundles.FindAsync(id);
    }
}

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

    public async Task<IReadOnlyList<LessonBundle>> GetAllAsync()
    {
        return await _context.LessonBundles
            .OrderByDescending(b => b.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LessonBundle> CreateAsync(LessonBundle bundle)
    {
        _context.LessonBundles.Add(bundle);
        await _context.SaveChangesAsync();
        return bundle;
    }

    public async Task<LessonBundle> UpdateAsync(LessonBundle bundle)
    {
        _context.LessonBundles.Update(bundle);
        await _context.SaveChangesAsync();
        return bundle;
    }

    public async Task<LessonBundle> UpdateIsActiveAsync(int bundleId, bool isActive)
    {
        await _context.LessonBundles
            .Where(b => b.Id == bundleId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(b => b.IsActive, isActive));
        return (await _context.LessonBundles.FindAsync(bundleId))!;
    }
}

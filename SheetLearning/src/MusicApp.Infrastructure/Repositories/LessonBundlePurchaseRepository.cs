using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class LessonBundlePurchaseRepository : ILessonBundlePurchaseRepository
{
    private readonly AppDbContext _context;

    public LessonBundlePurchaseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LessonBundlePurchase>> GetByUserAsync(int userId)
    {
        return await _context.LessonBundlePurchases
            .Include(p => p.Bundle)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LessonBundlePurchase?> GetByIdAsync(int id)
    {
        return await _context.LessonBundlePurchases
            .Include(p => p.Bundle)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<LessonBundlePurchase> UpdateAsync(LessonBundlePurchase purchase)
    {
        _context.LessonBundlePurchases.Update(purchase);
        await _context.SaveChangesAsync();
        return purchase;
    }
}

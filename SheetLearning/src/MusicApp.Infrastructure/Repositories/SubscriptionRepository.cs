using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetActiveByUserAsync(int userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _context.Subscriptions
            .Include(s => s.Plan)
            .Where(s => s.UserId == userId
                     && s.IsActive
                     && s.DataFine >= today)
            .OrderByDescending(s => s.DataInizio)
            .FirstOrDefaultAsync();
    }

    public async Task<Subscription> CreateAsync(Subscription subscription)
    {
        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }

    public async Task<Subscription> UpdateAsync(Subscription subscription)
    {
        _context.Subscriptions.Update(subscription);
        await _context.SaveChangesAsync();
        return subscription;
    }
}

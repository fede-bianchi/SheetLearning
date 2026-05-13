using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedAsync(
        int userId, int page, int pageSize)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId && !n.IsArchived)
            .OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }

    public async Task<int> GetUnreadCountAsync(int userId)
    {
        return await _context.Notifications
            .CountAsync(n =>
                n.UserId == userId &&
                !n.IsRead &&
                !n.IsArchived);
    }

    public async Task<Notification?> GetByIdAsync(int id)
    {
        return await _context.Notifications.FindAsync(id);
    }

    public async Task<Notification> CreateAsync(Notification notification)
    {
        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    public async Task MarkReadAsync(int notificationId, DateTime readAt)
    {
        await _context.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, readAt));
    }

    public async Task MarkAllReadAsync(int userId, DateTime readAt)
    {
        await _context.Notifications
            .Where(n =>
                n.UserId == userId &&
                !n.IsRead &&
                !n.IsArchived)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, readAt));
    }

    public async Task ArchiveAsync(int notificationId, DateTime archivedAt)
    {
        await _context.Notifications
            .Where(n => n.Id == notificationId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.IsArchived, true)
                .SetProperty(n => n.IsRead, true)
                .SetProperty(n => n.ReadAt, b => b.ReadAt ?? archivedAt));
    }
}

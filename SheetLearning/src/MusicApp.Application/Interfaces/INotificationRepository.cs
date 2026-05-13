using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface INotificationRepository
{
    Task<(IReadOnlyList<Notification> Items, int TotalCount)> GetPagedAsync(
        int userId, int page, int pageSize);

    Task<int> GetUnreadCountAsync(int userId);

    Task<Notification?> GetByIdAsync(int id);

    Task<Notification> CreateAsync(Notification notification);

    Task MarkReadAsync(int notificationId, DateTime readAt);

    Task MarkAllReadAsync(int userId, DateTime readAt);

    Task ArchiveAsync(int notificationId, DateTime archivedAt);
}

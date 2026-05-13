using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Notifications;

public class MarkAllNotificationsReadHandler
{
    private readonly INotificationRepository _notificationRepository;

    public MarkAllNotificationsReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<bool>> HandleAsync(int userId)
    {
        await _notificationRepository.MarkAllReadAsync(userId, DateTime.UtcNow);
        return Result<bool>.Ok(true);
    }
}

using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Notifications;

public class MarkNotificationReadHandler
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationReadHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<bool>> HandleAsync(int userId, int notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification is null || notification.UserId != userId)
            return Result<bool>.Fail(ErrorCodes.NotificationNotFound, "Notifica non trovata.");

        if (notification.IsRead)
            return Result<bool>.Ok(true);

        await _notificationRepository.MarkReadAsync(notificationId, DateTime.UtcNow);
        return Result<bool>.Ok(true);
    }
}

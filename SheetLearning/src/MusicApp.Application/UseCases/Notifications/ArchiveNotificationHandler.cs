using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Notifications;

public class ArchiveNotificationHandler
{
    private readonly INotificationRepository _notificationRepository;

    public ArchiveNotificationHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<bool>> HandleAsync(int userId, int notificationId)
    {
        var notification = await _notificationRepository.GetByIdAsync(notificationId);
        if (notification is null || notification.UserId != userId)
            return Result<bool>.Fail(ErrorCodes.NotificationNotFound, "Notifica non trovata.");

        if (notification.IsArchived)
            return Result<bool>.Ok(true);

        await _notificationRepository.ArchiveAsync(notificationId, DateTime.UtcNow);
        return Result<bool>.Ok(true);
    }
}

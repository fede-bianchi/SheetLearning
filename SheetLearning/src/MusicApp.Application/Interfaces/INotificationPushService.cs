using MusicApp.Application.DTOs;

namespace MusicApp.Application.Interfaces;

public interface INotificationPushService
{
    Task PushAsync(int recipientUserId, NotificationDto notification);
    Task PushUnreadCountAsync(int recipientUserId, int newCount);
}

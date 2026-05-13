using Microsoft.AspNetCore.SignalR;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Web.Hubs;

namespace MusicApp.Web.Services;

public class SignalRNotificationPushService : INotificationPushService
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotificationPushService(IHubContext<NotificationHub> hub)
        => _hub = hub;

    public Task PushAsync(int recipientUserId, NotificationDto notification)
        => _hub.Clients
               .Group($"notif_user_{recipientUserId}")
               .SendAsync("ReceiveNotification", notification);

    public Task PushUnreadCountAsync(int recipientUserId, int newCount)
        => _hub.Clients
               .Group($"notif_user_{recipientUserId}")
               .SendAsync("UnreadCountUpdated", newCount);
}

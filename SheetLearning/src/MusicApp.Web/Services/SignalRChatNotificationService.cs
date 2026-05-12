using Microsoft.AspNetCore.SignalR;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Web.Hubs;

namespace MusicApp.Web.Services;

public class SignalRChatNotificationService : IChatNotificationService
{
    private readonly IHubContext<ChatHub> _hubContext;

    public SignalRChatNotificationService(IHubContext<ChatHub> hubContext)
        => _hubContext = hubContext;

    public async Task NotifyNewMessageAsync(int recipientUserId, ChatMessageDto message)
    {
        await _hubContext.Clients
            .Group($"user_{recipientUserId}")
            .SendAsync("ReceiveMessage", message);
    }

    public async Task NotifyMessagesReadAsync(int recipientUserId, int chatId)
    {
        await _hubContext.Clients
            .Group($"user_{recipientUserId}")
            .SendAsync("MessagesRead", chatId);
    }
}

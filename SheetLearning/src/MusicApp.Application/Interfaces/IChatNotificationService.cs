using MusicApp.Application.DTOs;

namespace MusicApp.Application.Interfaces;

public interface IChatNotificationService
{
    Task NotifyNewMessageAsync(int recipientUserId, ChatMessageDto message);
    Task NotifyMessagesReadAsync(int recipientUserId, int chatId);
}

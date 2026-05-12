using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Chat;

public class MarkMessagesReadHandler
{
    private readonly IChatMessageRepository _messageRepo;
    private readonly IChatRepository _chatRepo;
    private readonly IChatNotificationService _notificationService;

    public MarkMessagesReadHandler(
        IChatMessageRepository messageRepo,
        IChatRepository chatRepo,
        IChatNotificationService notificationService)
    {
        _messageRepo = messageRepo;
        _chatRepo = chatRepo;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> HandleAsync(
        int chatId, int currentUserId)
    {
        var chat = await _chatRepo.GetByIdAsync(chatId);
        if (chat == null)
            return Result<bool>.Fail(
                ErrorCodes.ChatNotFound, "Chat non trovata.");

        int updated = await _messageRepo.MarkAllReadAsync(chatId, currentUserId);

        if (updated > 0)
        {
            int otherUserId = (currentUserId == chat.StudentId)
                ? chat.TeacherId
                : chat.StudentId;
            _ = _notificationService.NotifyMessagesReadAsync(otherUserId, chatId);
        }

        return Result<bool>.Ok(true);
    }
}

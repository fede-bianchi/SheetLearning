using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Chat;

public class GetMyChatsHandler
{
    private readonly IChatRepository _chatRepo;
    private readonly IChatMessageRepository _messageRepo;

    public GetMyChatsHandler(
        IChatRepository chatRepo,
        IChatMessageRepository messageRepo)
    {
        _chatRepo = chatRepo;
        _messageRepo = messageRepo;
    }

    public async Task<Result<IReadOnlyList<ChatSummaryDto>>> HandleAsync(int userId)
    {
        var chats = await _chatRepo.GetByUserAsync(userId);

        var chatIds = chats.Select(c => c.Id).ToList();

        var unreadCounts = chatIds.Count > 0
            ? await _messageRepo.GetUnreadCountBatchAsync(chatIds, userId)
            : new Dictionary<int, int>();
        var lastMessages = chatIds.Count > 0
            ? await _messageRepo.GetLastMessageBatchAsync(chatIds)
            : new Dictionary<int, string?>();

        var dtos = chats.Select(chat => new ChatSummaryDto(
            chat.Id,
            chat.StudentId,
            chat.Student?.Nickname ?? string.Empty,
            chat.TeacherId,
            chat.Teacher?.Nickname ?? string.Empty,
            unreadCounts.GetValueOrDefault(chat.Id, 0),
            lastMessages.GetValueOrDefault(chat.Id),
            chat.LastMessageAt,
            chat.CreatedAt
        )).ToList();

        return Result<IReadOnlyList<ChatSummaryDto>>.Ok(dtos);
    }
}

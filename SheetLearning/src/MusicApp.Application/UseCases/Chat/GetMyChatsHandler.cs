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

        var dtos = new List<ChatSummaryDto>();
        foreach (var chat in chats)
        {
            var unread = await _messageRepo.GetUnreadCountAsync(chat.Id, userId);
            var lastMsg = await _messageRepo.GetLastMessageAsync(chat.Id);

            dtos.Add(new ChatSummaryDto(
                chat.Id,
                chat.StudentId,
                chat.Student?.Nickname ?? string.Empty,
                chat.TeacherId,
                chat.Teacher?.Nickname ?? string.Empty,
                unread,
                lastMsg?.Contenuto,
                chat.LastMessageAt,
                chat.CreatedAt
            ));
        }

        return Result<IReadOnlyList<ChatSummaryDto>>.Ok(dtos);
    }
}

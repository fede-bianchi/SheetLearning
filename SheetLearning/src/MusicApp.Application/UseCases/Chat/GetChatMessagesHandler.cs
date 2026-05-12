using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Chat;

public class GetChatMessagesHandler
{
    private readonly IChatMessageRepository _messageRepo;

    public GetChatMessagesHandler(IChatMessageRepository messageRepo)
    {
        _messageRepo = messageRepo;
    }

    public async Task<Result<ChatMessagesResponse>> HandleAsync(
        int chatId, int page, int pageSize)
    {
        var clampedPageSize = Math.Min(pageSize, 50);

        var (messages, totalCount) = await _messageRepo.GetPagedAsync(
            chatId, page, clampedPageSize);

        var dtos = messages.Select(m => new ChatMessageDto(
            m.Id,
            m.ChatId,
            m.SenderId,
            m.Sender?.Nickname ?? string.Empty,
            m.Contenuto,
            m.Letto,
            m.CreatedAt
        )).ToList();

        var response = new ChatMessagesResponse(
            dtos, totalCount, page, clampedPageSize);

        return Result<ChatMessagesResponse>.Ok(response);
    }
}

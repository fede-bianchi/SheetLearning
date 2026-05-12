namespace MusicApp.Application.DTOs;

public record ChatMessagesResponse(
    IReadOnlyList<ChatMessageDto> Messages,
    int TotalCount,
    int Page,
    int PageSize
);

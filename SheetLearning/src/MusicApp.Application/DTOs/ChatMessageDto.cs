namespace MusicApp.Application.DTOs;

public record ChatMessageDto(
    int      Id,
    int      ChatId,
    int      SenderId,
    string   SenderNickname,
    string   Contenuto,
    bool     Letto,
    DateTime CreatedAt
);

namespace MusicApp.Application.DTOs;

public record NotificationDto(
    int       Id,
    string    Tipo,
    string    Titolo,
    string?   Corpo,
    string?   TargetType,
    int?      TargetId,
    bool      IsRead,
    bool      IsArchived,
    DateTime  CreatedAt,
    DateTime? ReadAt
);

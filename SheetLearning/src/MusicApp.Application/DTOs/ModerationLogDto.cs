namespace MusicApp.Application.DTOs;

public record ModerationLogDto(
    int      Id,
    int      AdminId,
    string   AdminNickname,
    int?     TargetUserId,
    string?  TargetUserNickname,
    string   Azione,
    string?  TargetType,
    int?     TargetId,
    string   Motivazione,
    string?  ContenutoRimosso,
    DateTime CreatedAt
);

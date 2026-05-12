namespace MusicApp.Application.DTOs;

public record ChatSummaryDto(
    int      Id,
    int      StudentId,
    string   StudentNickname,
    int      TeacherId,
    string   TeacherNickname,
    int      UnreadCount,
    string?  LastMessage,
    DateTime LastMessageAt,
    DateTime CreatedAt
);

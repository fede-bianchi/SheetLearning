namespace MusicApp.Application.DTOs;

public record FsrsElementStatDto(
    string   ElementType,
    string   Element,
    int      TotalErrors,
    int      RecentErrors,
    decimal  ErrorRate,
    DateTime FirstErrorAt,
    DateTime LastErrorAt
);

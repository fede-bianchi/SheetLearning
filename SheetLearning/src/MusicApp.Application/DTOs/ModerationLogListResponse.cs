namespace MusicApp.Application.DTOs;

public record ModerationLogListResponse(
    IReadOnlyList<ModerationLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

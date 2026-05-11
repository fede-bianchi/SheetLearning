namespace MusicApp.Application.DTOs;

public record PostListResponse(
    IReadOnlyList<PostSummaryDto> Posts,
    int TotalCount,
    int Page,
    int PageSize
);

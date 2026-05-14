namespace MusicApp.Application.DTOs;

public record AdminUserListResponse(
    IReadOnlyList<AdminUserDto> Users,
    int TotalCount,
    int Page,
    int PageSize
);

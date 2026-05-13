namespace MusicApp.Application.DTOs;

public record NotificationListResponse(
    IReadOnlyList<NotificationDto> Items,
    int TotalCount,
    int Page,
    int PageSize
);

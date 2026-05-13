namespace MusicApp.Application.DTOs;

public record NotificationQuery(
    int Page     = 1,
    int PageSize = 20
);

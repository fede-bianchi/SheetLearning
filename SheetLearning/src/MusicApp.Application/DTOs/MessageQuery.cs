namespace MusicApp.Application.DTOs;

public record MessageQuery(
    int Page     = 1,
    int PageSize = 20
);

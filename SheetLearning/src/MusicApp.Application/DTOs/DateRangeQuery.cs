namespace MusicApp.Application.DTOs;

public record DateRangeQuery(
    DateOnly? Dal = null,
    DateOnly? Al  = null
);

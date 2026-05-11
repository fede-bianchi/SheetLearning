namespace MusicApp.Application.DTOs;

public record LessonBundleDto(
    int     Id,
    string  Nome,
    int     NumeroLezioni,
    decimal Prezzo,
    decimal ScontoPercentuale,
    bool    IsActive,
    int?    ExpiresAfterDays
);

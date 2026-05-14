namespace MusicApp.Application.DTOs;

public record UpdateBundleRequest(
    string  NomeBundle,
    int     NumeroLezioni,
    decimal Prezzo,
    decimal ScontoPercentuale,
    int?    ExpiresAfterDays
);

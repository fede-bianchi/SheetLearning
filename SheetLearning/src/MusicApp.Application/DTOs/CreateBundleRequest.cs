namespace MusicApp.Application.DTOs;

public record CreateBundleRequest(
    string  NomeBundle,
    int     NumeroLezioni,
    decimal Prezzo,
    decimal ScontoPercentuale = 0m,
    int?    ExpiresAfterDays  = null
);

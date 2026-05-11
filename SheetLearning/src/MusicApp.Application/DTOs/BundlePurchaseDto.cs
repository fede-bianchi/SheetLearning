namespace MusicApp.Application.DTOs;

public record BundlePurchaseDto(
    int       Id,
    int       BundleId,
    string    NomeBundle,
    int       LezioniTotali,
    int       LezioniUsate,
    int       LezioniRimanenti,
    DateTime  CreatedAt,
    DateTime? ExpiresAt
);

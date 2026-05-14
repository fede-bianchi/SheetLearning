namespace MusicApp.Application.DTOs;

public record PlatformStatsDto(
    int     TotaleUtentiAttivi,
    int     TotaleUtentiBannati,
    int     TotaleInsegnanti,
    int     TotaleUtentiPro,
    int     TotalePost,
    int     TotaleCommenti,
    int     TotalePrenotazioni,
    int     TotaleTentativi,
    int     TotaleIscrizioniAttive,
    decimal EntrateUltimoMese
);

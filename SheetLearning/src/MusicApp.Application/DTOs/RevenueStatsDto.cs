namespace MusicApp.Application.DTOs;

public record RevenueStatsDto(
    DateOnly                  PeriodoDal,
    DateOnly                  PeriodoAl,
    decimal                   TotaleEntrate,
    Dictionary<string, decimal> EntratePerTipo,
    int                       PagamentiCompletati,
    int                       PagamentiFalliti,
    int                       PagamentiPending
);

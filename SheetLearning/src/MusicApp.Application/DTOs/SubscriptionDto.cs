namespace MusicApp.Application.DTOs;

public record SubscriptionDto(
    int      Id,
    int      PlanId,
    string   NomePiano,
    DateOnly DataInizio,
    DateOnly DataFine,
    bool     IsActive,
    bool     RinnovoAutomatico
);

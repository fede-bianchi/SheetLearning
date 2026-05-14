namespace MusicApp.Application.DTOs;

public record ModerationLogQuery(
    string?   FiltroAzione  = null,
    int?      FiltroAdminId = null,
    DateOnly? Dal           = null,
    DateOnly? Al            = null,
    int       Page          = 1,
    int       PageSize      = 20
);

namespace MusicApp.Application.DTOs;

public record AdminUserQuery(
    string?  FiltroRuolo    = null,
    bool?    FiltroAttivo   = null,
    string?  FiltroPiano    = null,
    string?  SearchQuery    = null,
    int      Page           = 1,
    int      PageSize       = 20
);

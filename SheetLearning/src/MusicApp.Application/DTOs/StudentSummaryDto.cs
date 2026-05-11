namespace MusicApp.Application.DTOs;

public record StudentSummaryDto(
    int     Id,
    string  Nickname,
    string  Nome,
    string  Cognome,
    string? Strumento
);

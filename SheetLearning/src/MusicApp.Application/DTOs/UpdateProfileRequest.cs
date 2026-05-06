namespace MusicApp.Application.DTOs;

public record UpdateProfileRequest(
    string? Nome,
    string? Cognome,
    string? Nickname,
    string? Descrizione,
    string? Strumento
);

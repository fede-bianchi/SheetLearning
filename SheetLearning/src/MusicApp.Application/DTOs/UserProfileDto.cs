namespace MusicApp.Application.DTOs;

public record UserProfileDto(
    int Id,
    string Nome,
    string Cognome,
    string Nickname,
    string Email,
    string Ruolo,
    string Piano,
    bool IsActive,
    string? Strumento,
    string? Descrizione,
    DateOnly DataNascita,
    DateTime CreatedAt
);

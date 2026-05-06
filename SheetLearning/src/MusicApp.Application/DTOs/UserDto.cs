namespace MusicApp.Application.DTOs;

public record UserDto(
    int Id,
    string Nome,
    string Cognome,
    string Nickname,
    string Email,
    string Ruolo,
    string Piano,
    bool IsActive,
    string? Strumento
);

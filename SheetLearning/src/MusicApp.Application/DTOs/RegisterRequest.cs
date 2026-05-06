namespace MusicApp.Application.DTOs;

public record RegisterRequest(
    string Nome,
    string Cognome,
    string Nickname,
    string Email,
    string Password,
    DateOnly DataNascita,
    string? Strumento
);

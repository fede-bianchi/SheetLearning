namespace MusicApp.Application.DTOs;

public record PublicUserDto(
    int Id,
    string Nickname,
    string? Strumento,
    string? Descrizione
);

namespace MusicApp.Application.DTOs;

public record TeacherStudentDetailDto(
    int       Id,
    string    Nickname,
    string    Nome,
    string    Cognome,
    string?   Strumento,
    int       TotaleLezioni,
    DateTime? UltimaLezioneAt
);

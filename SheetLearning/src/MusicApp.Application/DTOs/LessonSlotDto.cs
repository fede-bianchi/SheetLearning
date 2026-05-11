namespace MusicApp.Application.DTOs;

public record LessonSlotDto(
    int      Id,
    int      TeacherId,
    DateTime DataOraInizio,
    DateTime DataOraFine,
    bool     IsAvailable
);

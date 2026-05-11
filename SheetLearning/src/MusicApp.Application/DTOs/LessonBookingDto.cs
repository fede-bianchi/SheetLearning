namespace MusicApp.Application.DTOs;

public record LessonBookingDto(
    int      Id,
    int      StudentId,
    string   StudentNickname,
    int      TeacherId,
    string   TeacherNickname,
    int      SlotId,
    DateTime DataOraInizio,
    DateTime DataOraFine,
    int?     BundlePurchaseId,
    string   Stato,
    string?  NoteStudente,
    string?  NoteInsegnante,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

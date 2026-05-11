using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Teachers;

public class GetStudentBookingsHandler
{
    private readonly ILessonBookingRepository _bookingRepository;

    public GetStudentBookingsHandler(ILessonBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<IReadOnlyList<LessonBookingDto>>> HandleAsync(
        int teacherId, int studentId)
    {
        var bookings = await _bookingRepository.GetByTeacherAndStudentAsync(
            teacherId, studentId);

        var dtos = bookings.Select(b => new LessonBookingDto(
            b.Id,
            b.StudentId,
            b.Student?.Nickname ?? string.Empty,
            b.TeacherId,
            b.Teacher?.Nickname ?? string.Empty,
            b.SlotId,
            b.Slot?.DataOraInizio ?? default,
            b.Slot?.DataOraFine ?? default,
            b.BundlePurchaseId,
            b.Stato,
            b.NoteStudente,
            b.NoteInsegnante,
            b.CreatedAt,
            b.UpdatedAt
        )).ToList();

        return Result<IReadOnlyList<LessonBookingDto>>.Ok(dtos);
    }
}

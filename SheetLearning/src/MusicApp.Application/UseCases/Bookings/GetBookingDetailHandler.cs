using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Bookings;

public class GetBookingDetailHandler
{
    private readonly ILessonBookingRepository _bookingRepository;

    public GetBookingDetailHandler(ILessonBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<LessonBookingDto>> HandleAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            return Result<LessonBookingDto>.Fail(
                ErrorCodes.BookingNotFound, "Prenotazione non trovata.");

        var dto = new LessonBookingDto(
            booking.Id,
            booking.StudentId,
            booking.Student?.Nickname ?? string.Empty,
            booking.TeacherId,
            booking.Teacher?.Nickname ?? string.Empty,
            booking.SlotId,
            booking.Slot?.DataOraInizio ?? default,
            booking.Slot?.DataOraFine ?? default,
            booking.BundlePurchaseId,
            booking.Stato,
            booking.NoteStudente,
            booking.NoteInsegnante,
            booking.CreatedAt,
            booking.UpdatedAt
        );

        return Result<LessonBookingDto>.Ok(dto);
    }
}

using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Bookings;

public class CompleteBookingHandler
{
    private readonly ILessonBookingRepository _bookingRepository;

    public CompleteBookingHandler(ILessonBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<bool>> HandleAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            return Result<bool>.Fail(
                ErrorCodes.BookingNotFound, "Prenotazione non trovata.");

        if (booking.Stato != "confermata")
            return Result<bool>.Fail(
                ErrorCodes.BookingInvalidTransition,
                "Can only complete a booking in 'confermata' state.");

        booking.Stato = "completata";
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking);

        return Result<bool>.Ok(true);
    }
}

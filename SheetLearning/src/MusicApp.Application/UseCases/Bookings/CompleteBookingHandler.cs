using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Bookings;

public class CompleteBookingHandler
{
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public CompleteBookingHandler(
        ILessonBookingRepository bookingRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
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

        // Phase 7 — Notification dispatch
        var teacher = await _userRepository.GetByIdAsync(booking.TeacherId);
        _ = _notificationService.SendLezioneCompletataAsync(
            booking,
            teacher?.Nickname ?? "Insegnante");

        return Result<bool>.Ok(true);
    }
}

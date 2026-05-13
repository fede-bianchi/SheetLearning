using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Bookings;

public class ConfirmBookingHandler
{
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;
    private readonly INotificationService _notificationService;

    public ConfirmBookingHandler(
        ILessonBookingRepository bookingRepository,
        IUserRepository userRepository,
        INotificationService notificationService)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<LessonBookingDto>> HandleAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            return Result<LessonBookingDto>.Fail(
                ErrorCodes.BookingNotFound, "Prenotazione non trovata.");

        if (booking.Stato != "proposta")
            return Result<LessonBookingDto>.Fail(
                ErrorCodes.BookingInvalidTransition,
                "Can only confirm a booking in 'proposta' state.");

        booking.Stato = "confermata";
        booking.UpdatedAt = DateTime.UtcNow;
        await _bookingRepository.UpdateAsync(booking);

        // Phase 7 — Notification dispatch
        var teacher = await _userRepository.GetByIdAsync(booking.TeacherId);
        _ = _notificationService.SendLezioneConfermataAsync(
            booking,
            teacher?.Nickname ?? "Insegnante");

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

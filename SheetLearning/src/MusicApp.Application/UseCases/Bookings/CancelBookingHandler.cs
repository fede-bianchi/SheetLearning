using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Bookings;

public class CancelBookingHandler
{
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly ILessonSlotRepository _slotRepository;
    private readonly ILessonBundlePurchaseRepository _bundlePurchaseRepository;

    public CancelBookingHandler(
        ILessonBookingRepository bookingRepository,
        ILessonSlotRepository slotRepository,
        ILessonBundlePurchaseRepository bundlePurchaseRepository)
    {
        _bookingRepository = bookingRepository;
        _slotRepository = slotRepository;
        _bundlePurchaseRepository = bundlePurchaseRepository;
    }

    public async Task<Result<bool>> HandleAsync(
        int bookingId, CancelBookingRequest request, int cancellerId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            return Result<bool>.Fail(
                ErrorCodes.BookingNotFound, "Prenotazione non trovata.");

        if (booking.Stato != "proposta" && booking.Stato != "confermata")
            return Result<bool>.Fail(
                ErrorCodes.BookingInvalidTransition,
                "Can only cancel a booking in 'proposta' or 'confermata' state.");

        if (cancellerId == booking.TeacherId && request.MotivazioneCancel != null)
        {
            booking.NoteInsegnante = string.IsNullOrEmpty(booking.NoteInsegnante)
                ? request.MotivazioneCancel
                : booking.NoteInsegnante + " | " + request.MotivazioneCancel;
        }
        else if (cancellerId == booking.StudentId && request.MotivazioneCancel != null)
        {
            booking.NoteStudente = string.IsNullOrEmpty(booking.NoteStudente)
                ? request.MotivazioneCancel
                : booking.NoteStudente + " | " + request.MotivazioneCancel;
        }

        booking.Stato = "cancellata";
        booking.UpdatedAt = DateTime.UtcNow;

        if (booking.Slot != null)
        {
            booking.Slot.IsAvailable = true;
            await _slotRepository.UpdateAsync(booking.Slot);
        }

        if (booking.BundlePurchaseId.HasValue)
        {
            var purchase = await _bundlePurchaseRepository.GetByIdAsync(
                booking.BundlePurchaseId.Value);
            if (purchase != null)
            {
                purchase.LezioniUsate = Math.Max(0, purchase.LezioniUsate - 1);
                await _bundlePurchaseRepository.UpdateAsync(purchase);
            }
        }

        await _bookingRepository.UpdateAsync(booking);
        return Result<bool>.Ok(true);
    }
}

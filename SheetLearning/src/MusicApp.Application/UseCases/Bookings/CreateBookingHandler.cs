using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Bookings;

public class CreateBookingHandler
{
    private readonly ILessonSlotRepository _slotRepository;
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly ILessonBundlePurchaseRepository _bundlePurchaseRepository;

    public CreateBookingHandler(
        ILessonSlotRepository slotRepository,
        ILessonBookingRepository bookingRepository,
        ILessonBundlePurchaseRepository bundlePurchaseRepository)
    {
        _slotRepository = slotRepository;
        _bookingRepository = bookingRepository;
        _bundlePurchaseRepository = bundlePurchaseRepository;
    }

    public async Task<Result<LessonBookingDto>> HandleAsync(
        int studentId, CreateBookingRequest request)
    {
        var slot = await _slotRepository.GetByIdAsync(request.SlotId);
        if (slot is null)
            return Result<LessonBookingDto>.Fail(
                ErrorCodes.SlotNotFound, "Slot non trovato.");

        if (!slot.IsAvailable)
            return Result<LessonBookingDto>.Fail(
                ErrorCodes.SlotNotAvailable, "Lo slot non e disponibile.");

        if (slot.TeacherId != request.TeacherId)
            return Result<LessonBookingDto>.Fail(
                ErrorCodes.SlotTeacherMismatch,
                "TeacherId non corrisponde al teacher dello slot.");

        if (studentId == slot.TeacherId)
            return Result<LessonBookingDto>.Fail(
                ErrorCodes.CannotBookOwnSlot,
                "Non puoi prenotare un tuo slot.");

        LessonBundlePurchase? purchase = null;
        if (request.BundlePurchaseId.HasValue)
        {
            purchase = await _bundlePurchaseRepository.GetByIdAsync(
                request.BundlePurchaseId.Value);

            if (purchase is null || purchase.UserId != studentId)
                return Result<LessonBookingDto>.Fail(
                    ErrorCodes.BundlePurchaseNotFound,
                    "Acquisto bundle non trovato.");

            if (purchase.LezioniUsate >= purchase.LezioniTotali)
                return Result<LessonBookingDto>.Fail(
                    ErrorCodes.BundleExhausted, "Bundle esaurito.");

            if (purchase.ExpiresAt.HasValue
                && purchase.ExpiresAt.Value <= DateTime.UtcNow)
                return Result<LessonBookingDto>.Fail(
                    ErrorCodes.BundleExpired, "Bundle scaduto.");
        }

        var booking = new LessonBooking
        {
            StudentId = studentId,
            TeacherId = slot.TeacherId,
            SlotId = slot.Id,
            BundlePurchaseId = request.BundlePurchaseId,
            Stato = "proposta",
            NoteStudente = request.NoteStudente,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        booking = await _bookingRepository.CreateAsync(booking);

        slot.IsAvailable = false;
        await _slotRepository.UpdateAsync(slot);

        if (purchase != null)
        {
            purchase.LezioniUsate += 1;
            await _bundlePurchaseRepository.UpdateAsync(purchase);
        }

        var reloaded = await _bookingRepository.GetByIdAsync(booking.Id);

        var dto = new LessonBookingDto(
            reloaded!.Id,
            reloaded.StudentId,
            reloaded.Student?.Nickname ?? string.Empty,
            reloaded.TeacherId,
            reloaded.Teacher?.Nickname ?? string.Empty,
            reloaded.SlotId,
            reloaded.Slot?.DataOraInizio ?? slot.DataOraInizio,
            reloaded.Slot?.DataOraFine ?? slot.DataOraFine,
            reloaded.BundlePurchaseId,
            reloaded.Stato,
            reloaded.NoteStudente,
            reloaded.NoteInsegnante,
            reloaded.CreatedAt,
            reloaded.UpdatedAt
        );

        return Result<LessonBookingDto>.Ok(dto);
    }
}

namespace MusicApp.Application.DTOs;

public record CreateBookingRequest(
    int      SlotId,
    int      TeacherId,
    int?     BundlePurchaseId,
    string?  NoteStudente
);

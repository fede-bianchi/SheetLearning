using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Slots;

public class DeleteSlotHandler
{
    private readonly ILessonSlotRepository _slotRepository;

    public DeleteSlotHandler(ILessonSlotRepository slotRepository)
    {
        _slotRepository = slotRepository;
    }

    public async Task<Result<bool>> HandleAsync(int slotId)
    {
        var slot = await _slotRepository.GetByIdAsync(slotId);
        if (slot is null)
            return Result<bool>.Fail(
                ErrorCodes.SlotNotFound, "Slot non trovato.");

        if (!slot.IsAvailable)
            return Result<bool>.Fail(
                ErrorCodes.SlotAlreadyBooked,
                "Cannot delete a slot that has been booked.");

        await _slotRepository.DeleteAsync(slot);
        return Result<bool>.Ok(true);
    }
}

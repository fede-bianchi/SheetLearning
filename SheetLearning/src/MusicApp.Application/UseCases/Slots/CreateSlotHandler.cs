using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Slots;

public class CreateSlotHandler
{
    private readonly ILessonSlotRepository _slotRepository;

    public CreateSlotHandler(ILessonSlotRepository slotRepository)
    {
        _slotRepository = slotRepository;
    }

    public async Task<Result<LessonSlotDto>> HandleAsync(
        int teacherId, CreateSlotRequest request)
    {
        var slot = new LessonSlot
        {
            TeacherId = teacherId,
            DataOraInizio = request.DataOraInizio,
            DataOraFine = request.DataOraFine,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        await _slotRepository.CreateAsync(slot);

        var dto = new LessonSlotDto(
            slot.Id,
            slot.TeacherId,
            slot.DataOraInizio,
            slot.DataOraFine,
            slot.IsAvailable
        );

        return Result<LessonSlotDto>.Ok(dto);
    }
}

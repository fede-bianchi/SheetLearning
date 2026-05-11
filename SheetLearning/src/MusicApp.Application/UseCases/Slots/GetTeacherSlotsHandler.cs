using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Slots;

public class GetTeacherSlotsHandler
{
    private readonly ILessonSlotRepository _slotRepository;

    public GetTeacherSlotsHandler(ILessonSlotRepository slotRepository)
    {
        _slotRepository = slotRepository;
    }

    public async Task<Result<IReadOnlyList<LessonSlotDto>>> HandleAsync(
        int teacherId, SlotQuery query)
    {
        var slots = await _slotRepository.GetAvailableByTeacherAsync(
            teacherId, query.Dal, query.Al);

        var dtos = slots.Select(s => new LessonSlotDto(
            s.Id,
            s.TeacherId,
            s.DataOraInizio,
            s.DataOraFine,
            s.IsAvailable
        )).ToList();

        return Result<IReadOnlyList<LessonSlotDto>>.Ok(dtos);
    }
}

using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ILessonSlotRepository
{
    Task<IReadOnlyList<LessonSlot>> GetAvailableByTeacherAsync(
        int teacherId, DateOnly? dal, DateOnly? al);

    Task<IReadOnlyList<LessonSlot>> GetAllByTeacherAsync(
        int teacherId, DateOnly? dal, DateOnly? al);

    Task<LessonSlot?> GetByIdAsync(int id);
    Task<LessonSlot> CreateAsync(LessonSlot slot);
    Task<LessonSlot> UpdateAsync(LessonSlot slot);
    Task DeleteAsync(LessonSlot slot);
}

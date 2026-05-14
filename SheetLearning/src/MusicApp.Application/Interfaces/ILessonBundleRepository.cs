using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ILessonBundleRepository
{
    Task<IReadOnlyList<LessonBundle>> GetActiveAsync();
    Task<LessonBundle?> GetByIdAsync(int id);

    Task<IReadOnlyList<LessonBundle>> GetAllAsync();
    Task<LessonBundle> CreateAsync(LessonBundle bundle);
    Task<LessonBundle> UpdateAsync(LessonBundle bundle);
    Task<LessonBundle> UpdateIsActiveAsync(int bundleId, bool isActive);
}

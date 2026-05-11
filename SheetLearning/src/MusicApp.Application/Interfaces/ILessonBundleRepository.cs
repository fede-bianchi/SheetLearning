using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ILessonBundleRepository
{
    Task<IReadOnlyList<LessonBundle>> GetActiveAsync();
    Task<LessonBundle?> GetByIdAsync(int id);
}

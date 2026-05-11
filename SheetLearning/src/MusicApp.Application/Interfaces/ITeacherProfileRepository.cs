using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ITeacherProfileRepository
{
    Task<IReadOnlyList<TeacherProfile>> GetVisibleAsync(
        string? planClaim, string? roleClaim, string? filtroCategoria);

    Task<TeacherProfile?> GetByUserIdAsync(int userId);
    Task<TeacherProfile?> GetByIdAsync(int teacherProfileId);

    Task<TeacherProfile> CreateAsync(TeacherProfile profile);
    Task<TeacherProfile> UpdateAsync(TeacherProfile profile);

    Task SetCategoriesAsync(int teacherProfileId, IEnumerable<string> categories);
}

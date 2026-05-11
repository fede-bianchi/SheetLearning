using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ILessonBookingRepository
{
    Task<IReadOnlyList<LessonBooking>> GetByUserAsync(
        int userId, DateOnly? dal, DateOnly? al);

    Task<LessonBooking?> GetByIdAsync(int id);

    Task<IReadOnlyList<LessonBooking>> GetByTeacherAndStudentAsync(
        int teacherId, int studentId);

    Task<IReadOnlyList<User>> GetStudentsByTeacherAsync(int teacherId);

    Task<int> CountCompletedAsync(int teacherId, int studentId);

    Task<LessonBooking?> GetLastCompletedAsync(int teacherId, int studentId);

    Task<LessonBooking> CreateAsync(LessonBooking booking);
    Task<LessonBooking> UpdateAsync(LessonBooking booking);
}

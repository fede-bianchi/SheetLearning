using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ILessonRatingRepository
{
    Task<LessonRating?> GetByBookingAsync(int bookingId);
    Task<LessonRating> CreateAsync(LessonRating rating);

    Task<IReadOnlyList<LessonRating>> GetByTeacherAsync(int teacherId);
}

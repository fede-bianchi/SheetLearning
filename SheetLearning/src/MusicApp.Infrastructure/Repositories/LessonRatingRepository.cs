using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class LessonRatingRepository : ILessonRatingRepository
{
    private readonly AppDbContext _context;

    public LessonRatingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LessonRating?> GetByBookingAsync(int bookingId)
    {
        return await _context.LessonRatings
            .FirstOrDefaultAsync(r => r.BookingId == bookingId);
    }

    public async Task<LessonRating> CreateAsync(LessonRating rating)
    {
        _context.LessonRatings.Add(rating);
        await _context.SaveChangesAsync();
        return rating;
    }

    public async Task<IReadOnlyList<LessonRating>> GetByTeacherAsync(int teacherId)
    {
        return await _context.LessonRatings
            .Where(r => r.TeacherId == teacherId)
            .AsNoTracking()
            .ToListAsync();
    }
}

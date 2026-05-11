using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class LessonBookingRepository : ILessonBookingRepository
{
    private readonly AppDbContext _context;

    public LessonBookingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LessonBooking>> GetByUserAsync(
        int userId, DateOnly? dal, DateOnly? al)
    {
        var query = _context.LessonBookings
            .Include(b => b.Slot)
            .Include(b => b.Student)
            .Include(b => b.Teacher)
            .Where(b => b.StudentId == userId || b.TeacherId == userId);

        if (dal.HasValue)
            query = query.Where(b =>
                DateOnly.FromDateTime(b.Slot.DataOraInizio) >= dal.Value);
        if (al.HasValue)
            query = query.Where(b =>
                DateOnly.FromDateTime(b.Slot.DataOraInizio) <= al.Value);

        return await query
            .OrderByDescending(b => b.Slot.DataOraInizio)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LessonBooking?> GetByIdAsync(int id)
    {
        return await _context.LessonBookings
            .Include(b => b.Slot)
            .Include(b => b.Student)
            .Include(b => b.Teacher)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IReadOnlyList<LessonBooking>> GetByTeacherAndStudentAsync(
        int teacherId, int studentId)
    {
        return await _context.LessonBookings
            .Include(b => b.Slot)
            .Include(b => b.Student)
            .Include(b => b.Teacher)
            .Where(b => b.TeacherId == teacherId && b.StudentId == studentId)
            .OrderByDescending(b => b.Slot.DataOraInizio)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<User>> GetStudentsByTeacherAsync(int teacherId)
    {
        return await _context.LessonBookings
            .Where(b => b.TeacherId == teacherId
                     && (b.Stato == "confermata" || b.Stato == "completata"))
            .Select(b => b.Student)
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CountCompletedAsync(int teacherId, int studentId)
    {
        return await _context.LessonBookings
            .CountAsync(b =>
                b.TeacherId == teacherId &&
                b.StudentId == studentId &&
                b.Stato == "completata");
    }

    public async Task<LessonBooking?> GetLastCompletedAsync(
        int teacherId, int studentId)
    {
        return await _context.LessonBookings
            .Where(b =>
                b.TeacherId == teacherId &&
                b.StudentId == studentId &&
                b.Stato == "completata")
            .OrderByDescending(b => b.UpdatedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<LessonBooking> CreateAsync(LessonBooking booking)
    {
        _context.LessonBookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task<LessonBooking> UpdateAsync(LessonBooking booking)
    {
        _context.LessonBookings.Update(booking);
        await _context.SaveChangesAsync();
        return booking;
    }
}

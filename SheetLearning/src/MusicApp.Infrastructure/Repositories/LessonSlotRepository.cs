using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class LessonSlotRepository : ILessonSlotRepository
{
    private readonly AppDbContext _context;

    public LessonSlotRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<LessonSlot>> GetAvailableByTeacherAsync(
        int teacherId, DateOnly? dal, DateOnly? al)
    {
        var query = _context.LessonSlots
            .Where(s => s.TeacherId == teacherId && s.IsAvailable);

        if (dal.HasValue)
            query = query.Where(s =>
                DateOnly.FromDateTime(s.DataOraInizio) >= dal.Value);
        if (al.HasValue)
            query = query.Where(s =>
                DateOnly.FromDateTime(s.DataOraInizio) <= al.Value);

        return await query
            .OrderBy(s => s.DataOraInizio)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<LessonSlot>> GetAllByTeacherAsync(
        int teacherId, DateOnly? dal, DateOnly? al)
    {
        var query = _context.LessonSlots
            .Where(s => s.TeacherId == teacherId);

        if (dal.HasValue)
            query = query.Where(s =>
                DateOnly.FromDateTime(s.DataOraInizio) >= dal.Value);
        if (al.HasValue)
            query = query.Where(s =>
                DateOnly.FromDateTime(s.DataOraInizio) <= al.Value);

        return await query
            .OrderBy(s => s.DataOraInizio)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LessonSlot?> GetByIdAsync(int id)
    {
        return await _context.LessonSlots.FindAsync(id);
    }

    public async Task<LessonSlot> CreateAsync(LessonSlot slot)
    {
        _context.LessonSlots.Add(slot);
        await _context.SaveChangesAsync();
        return slot;
    }

    public async Task<LessonSlot> UpdateAsync(LessonSlot slot)
    {
        _context.LessonSlots.Update(slot);
        await _context.SaveChangesAsync();
        return slot;
    }

    public async Task DeleteAsync(LessonSlot slot)
    {
        _context.LessonSlots.Remove(slot);
        await _context.SaveChangesAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class ModerationLogRepository : IModerationLogRepository
{
    private readonly AppDbContext _context;

    public ModerationLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ModerationLog> CreateAsync(ModerationLog log)
    {
        _context.ModerationLogs.Add(log);
        await _context.SaveChangesAsync();
        return log;
    }

    public async Task<(IReadOnlyList<ModerationLog> Items, int TotalCount)> GetPagedAsync(
        string?  filtroAzione,
        int?     filtroAdminId,
        DateOnly? dal,
        DateOnly? al,
        int      page,
        int      pageSize)
    {
        var query = _context.ModerationLogs
            .Include(l => l.Admin)
            .Include(l => l.TargetUser)
            .AsQueryable();

        if (filtroAzione != null)
            query = query.Where(l => l.Azione == filtroAzione);
        if (filtroAdminId.HasValue)
            query = query.Where(l => l.AdminId == filtroAdminId.Value);
        if (dal.HasValue)
            query = query.Where(l =>
                DateOnly.FromDateTime(l.CreatedAt) >= dal.Value);
        if (al.HasValue)
            query = query.Where(l =>
                DateOnly.FromDateTime(l.CreatedAt) <= al.Value);

        query = query.OrderByDescending(l => l.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (items, total);
    }
}

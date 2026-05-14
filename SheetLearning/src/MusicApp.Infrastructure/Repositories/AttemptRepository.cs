using Microsoft.EntityFrameworkCore;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class AttemptRepository : IAttemptRepository
{
    private readonly AppDbContext _context;

    public AttemptRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Attempt> CreateAsync(Attempt attempt)
    {
        _context.Attempts.Add(attempt);
        await _context.SaveChangesAsync();
        return attempt;
    }

    public async Task<IReadOnlyList<Attempt>> GetByUserAndTypeAsync(int userId, int? exerciseTypeId)
    {
        IQueryable<Attempt> query = _context.Attempts
            .Include(attempt => attempt.AttemptErrors)
            .Where(attempt => attempt.UserId == userId);

        if (exerciseTypeId.HasValue)
        {
            query = query.Where(attempt => attempt.ExerciseTypeId == exerciseTypeId.Value);
        }

        return await query
            .OrderByDescending(attempt => attempt.CreatedAt)
            .ThenByDescending(attempt => attempt.Id)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<int> CountByUserAndTypeAsync(int userId, int exerciseTypeId)
    {
        return _context.Attempts
            .CountAsync(attempt => attempt.UserId == userId && attempt.ExerciseTypeId == exerciseTypeId);
    }

    public async Task<IReadOnlyList<int>> GetOldestDeletableIdsAsync(
        int userId,
        int exerciseTypeId,
        IEnumerable<int> protectedAttemptIds,
        int excessCount)
    {
        var protectedIds = protectedAttemptIds.Distinct().ToArray();

        return await _context.Attempts
            .Where(attempt => attempt.UserId == userId
                              && attempt.ExerciseTypeId == exerciseTypeId
                              && !protectedIds.Contains(attempt.Id))
            .OrderBy(attempt => attempt.CreatedAt)
            .ThenBy(attempt => attempt.Id)
            .Take(excessCount)
            .Select(attempt => attempt.Id)
            .ToListAsync();
    }

    public async Task DeleteByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.Distinct().ToArray();
        if (idList.Length == 0)
        {
            return;
        }

        await _context.Attempts
            .Where(attempt => idList.Contains(attempt.Id))
            .ExecuteDeleteAsync();
    }

    public async Task<IReadOnlyList<FsrsElementStatDto>>
        GetFsrsAnalyticsAsync(int userId, int exerciseTypeId)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var totalAttempts = await _context.Attempts
            .CountAsync(a => a.UserId == userId
                          && a.ExerciseTypeId == exerciseTypeId);

        if (totalAttempts == 0)
            return Array.Empty<FsrsElementStatDto>();

        var stats = await _context.AttemptErrors
            .Join(_context.Attempts,
                  ae => ae.AttemptId,
                  a  => a.Id,
                  (ae, a) => new { ae, a })
            .Where(x => x.a.UserId == userId
                     && x.a.ExerciseTypeId == exerciseTypeId)
            .GroupBy(x => new
            {
                x.ae.ElementType,
                x.ae.RispostaCorretta
            })
            .Select(g => new
            {
                ElementType   = g.Key.ElementType,
                Element       = g.Key.RispostaCorretta,
                TotalErrors   = g.Count(),
                RecentErrors  = g.Count(x => x.a.CreatedAt >= thirtyDaysAgo),
                FirstErrorAt  = g.Min(x => x.a.CreatedAt),
                LastErrorAt   = g.Max(x => x.a.CreatedAt)
            })
            .OrderByDescending(x => x.TotalErrors)
            .AsNoTracking()
            .ToListAsync();

        return stats.Select(s => new FsrsElementStatDto(
            ElementType  : s.ElementType,
            Element      : s.Element,
            TotalErrors  : s.TotalErrors,
            RecentErrors : s.RecentErrors,
            ErrorRate    : Math.Round(
                               Math.Min(1m, (decimal)s.TotalErrors / totalAttempts),
                               4),
            FirstErrorAt : s.FirstErrorAt,
            LastErrorAt  : s.LastErrorAt
        )).ToList();
    }
}

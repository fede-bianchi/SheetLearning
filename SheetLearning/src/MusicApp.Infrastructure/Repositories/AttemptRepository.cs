using Microsoft.EntityFrameworkCore;
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
}

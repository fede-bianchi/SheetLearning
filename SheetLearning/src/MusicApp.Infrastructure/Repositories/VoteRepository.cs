using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class VoteRepository : IVoteRepository
{
    private readonly AppDbContext _context;

    public VoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Vote?> GetByUserAndTargetAsync(int userId, string targetType, int targetId)
    {
        return _context.Votes.FirstOrDefaultAsync(vote =>
            vote.UserId == userId
            && vote.TargetType == targetType
            && vote.TargetId == targetId);
    }

    public async Task<(int Upvotes, int Downvotes)> GetCountsAsync(string targetType, int targetId)
    {
        var votes = await _context.Votes
            .Where(vote => vote.TargetType == targetType && vote.TargetId == targetId)
            .ToListAsync();

        return (
            votes.Count(vote => vote.Voto == "upvote"),
            votes.Count(vote => vote.Voto == "downvote"));
    }

    public async Task<Dictionary<int, (int Upvotes, int Downvotes)>> GetCountsBatchAsync(
        string targetType,
        IEnumerable<int> targetIds)
    {
        var ids = targetIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<int, (int Upvotes, int Downvotes)>();
        }

        var groupedVotes = await _context.Votes
            .Where(vote => vote.TargetType == targetType && ids.Contains(vote.TargetId))
            .GroupBy(vote => vote.TargetId)
            .Select(group => new
            {
                TargetId = group.Key,
                Upvotes = group.Count(vote => vote.Voto == "upvote"),
                Downvotes = group.Count(vote => vote.Voto == "downvote")
            })
            .ToListAsync();

        return ids.ToDictionary(
            id => id,
            id => groupedVotes.FirstOrDefault(item => item.TargetId == id) is { } item
                ? (item.Upvotes, item.Downvotes)
                : (0, 0));
    }

    public async Task<Dictionary<int, string>> GetUserVotesBatchAsync(
        int userId,
        string targetType,
        IEnumerable<int> targetIds)
    {
        var ids = targetIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        var votes = await _context.Votes
            .Where(vote => vote.UserId == userId
                           && vote.TargetType == targetType
                           && ids.Contains(vote.TargetId))
            .ToListAsync();

        return votes.ToDictionary(vote => vote.TargetId, vote => vote.Voto);
    }

    public async Task<Vote> CreateAsync(Vote vote)
    {
        _context.Votes.Add(vote);
        await _context.SaveChangesAsync();
        return vote;
    }

    public async Task<Vote> UpdateAsync(Vote vote)
    {
        _context.Votes.Update(vote);
        await _context.SaveChangesAsync();
        return vote;
    }

    public async Task DeleteAsync(Vote vote)
    {
        _context.Votes.Remove(vote);
        await _context.SaveChangesAsync();
    }
}

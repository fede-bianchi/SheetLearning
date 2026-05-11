using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Post> Posts, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string orderBy)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 20 : pageSize;

        IQueryable<Post> query;

        if (orderBy == "votati")
        {
            query = _context.Posts
                .Include(post => post.User)
                .GroupJoin(
                    _context.Votes.Where(vote => vote.TargetType == "post"),
                    post => post.Id,
                    vote => vote.TargetId,
                    (post, votes) => new
                    {
                        Post = post,
                        NetVotes = votes.Count(vote => vote.Voto == "upvote")
                                   - votes.Count(vote => vote.Voto == "downvote")
                    })
                .OrderByDescending(item => item.NetVotes)
                .ThenByDescending(item => item.Post.CreatedAt)
                .Select(item => item.Post);
        }
        else
        {
            query = _context.Posts
                .Include(post => post.User)
                .OrderByDescending(post => post.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var posts = await query
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize)
            .AsNoTracking()
            .ToListAsync();

        return (posts, totalCount);
    }

    public Task<Post?> GetByIdAsync(int id)
    {
        return _context.Posts.FirstOrDefaultAsync(post => post.Id == id);
    }

    public Task<Post?> GetByIdWithAuthorAsync(int id)
    {
        return _context.Posts
            .Include(post => post.User)
            .FirstOrDefaultAsync(post => post.Id == id);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<Post> UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task SoftDeleteAsync(Post post)
    {
        post.IsDeleted = true;
        post.DeletedAt = DateTime.UtcNow;
        post.UpdatedAt = DateTime.UtcNow;

        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
    }
}

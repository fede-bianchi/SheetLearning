using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Comment>> GetAllByPostAsync(int postId)
    {
        return await _context.Comments
            .Include(comment => comment.User)
            .Where(comment => comment.PostId == postId)
            .OrderBy(comment => comment.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Comment>> GetTopLevelByPostAsync(int postId)
    {
        return await _context.Comments
            .Include(comment => comment.User)
            .Where(comment => comment.PostId == postId && comment.ParentCommentId == null)
            .OrderBy(comment => comment.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Comment?> GetByIdAsync(int id)
    {
        return _context.Comments.FirstOrDefaultAsync(comment => comment.Id == id);
    }

    public Task<Comment?> GetByIdWithAuthorAsync(int id)
    {
        return _context.Comments
            .Include(comment => comment.User)
            .FirstOrDefaultAsync(comment => comment.Id == id);
    }

    public async Task<Comment> CreateAsync(Comment comment)
    {
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task<Comment> UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task SoftDeleteAsync(Comment comment)
    {
        comment.IsDeleted = true;
        comment.DeletedAt = DateTime.UtcNow;
        comment.UpdatedAt = DateTime.UtcNow;

        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }
}

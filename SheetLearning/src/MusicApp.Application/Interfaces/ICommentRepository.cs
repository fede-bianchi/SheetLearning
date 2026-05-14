using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ICommentRepository
{
    Task<IReadOnlyList<Comment>> GetAllByPostAsync(int postId);
    Task<IReadOnlyList<Comment>> GetTopLevelByPostAsync(int postId);

    Task<Comment?> GetByIdAsync(int id);
    Task<Comment?> GetByIdWithAuthorAsync(int id);

    Task<Comment> CreateAsync(Comment comment);
    Task<Comment> UpdateAsync(Comment comment);
    Task SoftDeleteAsync(Comment comment);

    Task<Dictionary<int, int>> GetCountBatchAsync(IEnumerable<int> postIds);
}

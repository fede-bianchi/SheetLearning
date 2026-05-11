using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IPostRepository
{
    Task<(IReadOnlyList<Post> Posts, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string orderBy);

    Task<Post?> GetByIdAsync(int id);
    Task<Post?> GetByIdWithAuthorAsync(int id);

    Task<Post> CreateAsync(Post post);
    Task<Post> UpdateAsync(Post post);

    Task SoftDeleteAsync(Post post);
}

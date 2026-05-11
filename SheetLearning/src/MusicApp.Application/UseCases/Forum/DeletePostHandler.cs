using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class DeletePostHandler
{
    private readonly IPostRepository _postRepository;

    public DeletePostHandler(IPostRepository postRepository)
    {
        _postRepository = postRepository;
    }

    public async Task<Result<bool>> HandleAsync(int postId)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post is null)
        {
            return Result<bool>.Fail(ErrorCodes.PostNotFound, "Post non trovato.");
        }

        if (post.IsDeleted)
        {
            return Result<bool>.Ok(true);
        }

        await _postRepository.SoftDeleteAsync(post);
        return Result<bool>.Ok(true);
    }
}

using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class UpdatePostHandler
{
    private readonly IPostRepository _postRepository;
    private readonly IVoteRepository _voteRepository;

    public UpdatePostHandler(
        IPostRepository postRepository,
        IVoteRepository voteRepository)
    {
        _postRepository = postRepository;
        _voteRepository = voteRepository;
    }

    public async Task<Result<PostSummaryDto>> HandleAsync(int postId, UpdatePostRequest request)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post is null)
        {
            return Result<PostSummaryDto>.Fail(ErrorCodes.PostNotFound, "Post non trovato.");
        }

        if (post.IsDeleted)
        {
            return Result<PostSummaryDto>.Fail(ErrorCodes.PostDeleted, "Cannot edit a deleted post.");
        }

        post.Titolo = request.Titolo;
        post.Contenuto = request.Contenuto;
        post.UpdatedAt = DateTime.UtcNow;

        await _postRepository.UpdateAsync(post);

        var postWithAuthor = await _postRepository.GetByIdWithAuthorAsync(postId);
        if (postWithAuthor is null)
        {
            return Result<PostSummaryDto>.Fail(ErrorCodes.PostNotFound, "Post non trovato.");
        }

        var (upvotes, downvotes) = await _voteRepository.GetCountsAsync("post", postId);

        return Result<PostSummaryDto>.Ok(postWithAuthor.ToPostSummaryDto(
            upvotes,
            downvotes,
            userVote: null,
            commentCount: 0));
    }
}

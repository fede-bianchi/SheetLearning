using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class GetPostDetailHandler
{
    private readonly IPostRepository _postRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly IVoteRepository _voteRepository;

    public GetPostDetailHandler(
        IPostRepository postRepository,
        ICommentRepository commentRepository,
        IVoteRepository voteRepository)
    {
        _postRepository = postRepository;
        _commentRepository = commentRepository;
        _voteRepository = voteRepository;
    }

    public async Task<Result<PostDetailDto>> HandleAsync(int postId, int? currentUserId)
    {
        var post = await _postRepository.GetByIdWithAuthorAsync(postId);
        if (post is null)
        {
            return Result<PostDetailDto>.Fail(ErrorCodes.PostNotFound, "Post non trovato.");
        }

        var (postUpvotes, postDownvotes) = await _voteRepository.GetCountsAsync("post", post.Id);

        string? postUserVote = null;
        if (currentUserId.HasValue)
        {
            var existingVote = await _voteRepository.GetByUserAndTargetAsync(currentUserId.Value, "post", post.Id);
            postUserVote = existingVote?.Voto;
        }

        var topLevelComments = await _commentRepository.GetTopLevelByPostAsync(postId);
        var commentIds = topLevelComments.Select(comment => comment.Id).ToList();

        var voteCountsByCommentId = commentIds.Count > 0
            ? await _voteRepository.GetCountsBatchAsync("comment", commentIds)
            : new Dictionary<int, (int Upvotes, int Downvotes)>();

        var userVotesByCommentId = currentUserId.HasValue && commentIds.Count > 0
            ? await _voteRepository.GetUserVotesBatchAsync(currentUserId.Value, "comment", commentIds)
            : new Dictionary<int, string>();

        var topLevelCommentDtos = topLevelComments
            .Select(comment =>
            {
                var counts = voteCountsByCommentId.TryGetValue(comment.Id, out var item)
                    ? item
                    : (Upvotes: 0, Downvotes: 0);

                userVotesByCommentId.TryGetValue(comment.Id, out var userVote);

                return comment.ToCommentDto(
                    counts.Upvotes,
                    counts.Downvotes,
                    userVote,
                    []);
            })
            .ToList();

        return Result<PostDetailDto>.Ok(post.ToPostDetailDto(
            postUpvotes,
            postDownvotes,
            postUserVote,
            topLevelCommentDtos));
    }
}

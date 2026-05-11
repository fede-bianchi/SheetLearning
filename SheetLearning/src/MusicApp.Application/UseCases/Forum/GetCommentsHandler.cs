using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Helpers;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class GetCommentsHandler
{
    private readonly ICommentRepository _commentRepository;
    private readonly IVoteRepository _voteRepository;

    public GetCommentsHandler(
        ICommentRepository commentRepository,
        IVoteRepository voteRepository)
    {
        _commentRepository = commentRepository;
        _voteRepository = voteRepository;
    }

    public async Task<Result<IReadOnlyList<CommentDto>>> HandleAsync(int postId, int? currentUserId)
    {
        var flatComments = await _commentRepository.GetAllByPostAsync(postId);
        if (flatComments.Count == 0)
        {
            return Result<IReadOnlyList<CommentDto>>.Ok([]);
        }

        var commentIds = flatComments.Select(comment => comment.Id).ToList();
        var voteCountsByCommentId = await _voteRepository.GetCountsBatchAsync("comment", commentIds);

        var userVotesByCommentId = currentUserId.HasValue
            ? await _voteRepository.GetUserVotesBatchAsync(currentUserId.Value, "comment", commentIds)
            : new Dictionary<int, string>();

        CommentDto MapComment(
            MusicApp.Domain.Entities.Comment comment,
            IReadOnlyList<CommentDto> risposte)
        {
            var counts = voteCountsByCommentId.TryGetValue(comment.Id, out var item)
                ? item
                : (Upvotes: 0, Downvotes: 0);

            userVotesByCommentId.TryGetValue(comment.Id, out var userVote);

            return comment.ToCommentDto(
                counts.Upvotes,
                counts.Downvotes,
                userVote,
                risposte);
        }

        var tree = CommentTreeBuilder.Build(flatComments, MapComment);
        return Result<IReadOnlyList<CommentDto>>.Ok(tree);
    }
}

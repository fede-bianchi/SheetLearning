using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class UpdateCommentHandler
{
    private readonly ICommentRepository _commentRepository;
    private readonly IVoteRepository _voteRepository;

    public UpdateCommentHandler(
        ICommentRepository commentRepository,
        IVoteRepository voteRepository)
    {
        _commentRepository = commentRepository;
        _voteRepository = voteRepository;
    }

    public async Task<Result<CommentDto>> HandleAsync(int commentId, UpdateCommentRequest request)
    {
        var comment = await _commentRepository.GetByIdWithAuthorAsync(commentId);
        if (comment is null)
        {
            return Result<CommentDto>.Fail(ErrorCodes.CommentNotFound, "Commento non trovato.");
        }

        if (comment.IsDeleted)
        {
            return Result<CommentDto>.Fail(ErrorCodes.CommentDeleted, "Cannot edit a deleted comment.");
        }

        comment.Contenuto = request.Contenuto;
        comment.UpdatedAt = DateTime.UtcNow;

        await _commentRepository.UpdateAsync(comment);

        var updatedWithAuthor = await _commentRepository.GetByIdWithAuthorAsync(commentId);
        if (updatedWithAuthor is null)
        {
            return Result<CommentDto>.Fail(ErrorCodes.CommentNotFound, "Commento non trovato.");
        }

        var (upvotes, downvotes) = await _voteRepository.GetCountsAsync("comment", commentId);

        return Result<CommentDto>.Ok(updatedWithAuthor.ToCommentDto(
            upvotes,
            downvotes,
            userVote: null,
            risposte: []));
    }
}

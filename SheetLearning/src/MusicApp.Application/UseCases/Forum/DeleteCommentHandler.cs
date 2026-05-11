using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Forum;

public class DeleteCommentHandler
{
    private readonly ICommentRepository _commentRepository;

    public DeleteCommentHandler(ICommentRepository commentRepository)
    {
        _commentRepository = commentRepository;
    }

    public async Task<Result<bool>> HandleAsync(int commentId)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment is null)
        {
            return Result<bool>.Fail(ErrorCodes.CommentNotFound, "Commento non trovato.");
        }

        if (comment.IsDeleted)
        {
            return Result<bool>.Ok(true);
        }

        await _commentRepository.SoftDeleteAsync(comment);
        return Result<bool>.Ok(true);
    }
}

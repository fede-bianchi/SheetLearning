using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Forum;

public class CreateCommentHandler
{
    private readonly ICommentRepository _commentRepository;
    private readonly IPostRepository _postRepository;

    public CreateCommentHandler(
        ICommentRepository commentRepository,
        IPostRepository postRepository)
    {
        _commentRepository = commentRepository;
        _postRepository = postRepository;
    }

    public async Task<Result<CommentDto>> HandleAsync(int userId, int postId, CreateCommentRequest request)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post is null)
        {
            return Result<CommentDto>.Fail(ErrorCodes.PostNotFound, "Post non trovato.");
        }

        if (post.IsDeleted)
        {
            return Result<CommentDto>.Fail(ErrorCodes.PostDeleted, "Cannot comment on a deleted post.");
        }

        if (request.ParentCommentId.HasValue)
        {
            var parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value);
            if (parentComment is null)
            {
                return Result<CommentDto>.Fail(ErrorCodes.CommentNotFound, "Parent comment not found.");
            }

            if (parentComment.PostId != postId)
            {
                return Result<CommentDto>.Fail(
                    ErrorCodes.CommentNotFound,
                    "Parent comment does not belong to this post.");
            }
        }

        var now = DateTime.UtcNow;
        var comment = new Comment
        {
            PostId = postId,
            UserId = userId,
            ParentCommentId = request.ParentCommentId,
            Contenuto = request.Contenuto,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _commentRepository.CreateAsync(comment);
        var createdWithAuthor = await _commentRepository.GetByIdWithAuthorAsync(created.Id);
        if (createdWithAuthor is null)
        {
            return Result<CommentDto>.Fail(ErrorCodes.CommentNotFound, "Commento non trovato.");
        }

        return Result<CommentDto>.Ok(createdWithAuthor.ToCommentDto(
            upvotes: 0,
            downvotes: 0,
            userVote: null,
            risposte: []));
    }
}

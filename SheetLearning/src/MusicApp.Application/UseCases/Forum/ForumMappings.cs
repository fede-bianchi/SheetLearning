using MusicApp.Application.DTOs;
using MusicApp.Application.Helpers;
using MusicApp.Application.UseCases;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Forum;

internal static class ForumMappings
{
    public static PostSummaryDto ToPostSummaryDto(
        this Post post,
        int upvotes,
        int downvotes,
        string? userVote,
        int commentCount)
    {
        return new PostSummaryDto(
            post.Id,
            SoftDeleteContentHelper.ResolvePostTitolo(post),
            SoftDeleteContentHelper.ResolvePostContenuto(post),
            post.User.ToPublicUserDto(),
            upvotes,
            downvotes,
            userVote,
            commentCount,
            post.IsDeleted,
            post.CreatedAt,
            post.UpdatedAt);
    }

    public static CommentDto ToCommentDto(
        this Comment comment,
        int upvotes,
        int downvotes,
        string? userVote,
        IReadOnlyList<CommentDto> risposte)
    {
        return new CommentDto(
            comment.Id,
            comment.PostId,
            comment.ParentCommentId,
            SoftDeleteContentHelper.ResolveCommentContenuto(comment),
            comment.User.ToPublicUserDto(),
            upvotes,
            downvotes,
            userVote,
            comment.IsDeleted,
            comment.CreatedAt,
            comment.UpdatedAt,
            risposte);
    }

    public static PostDetailDto ToPostDetailDto(
        this Post post,
        int upvotes,
        int downvotes,
        string? userVote,
        IReadOnlyList<CommentDto> topLevelComments)
    {
        return new PostDetailDto(
            post.Id,
            SoftDeleteContentHelper.ResolvePostTitolo(post),
            SoftDeleteContentHelper.ResolvePostContenuto(post),
            post.User.ToPublicUserDto(),
            upvotes,
            downvotes,
            userVote,
            post.IsDeleted,
            post.CreatedAt,
            post.UpdatedAt,
            topLevelComments);
    }
}

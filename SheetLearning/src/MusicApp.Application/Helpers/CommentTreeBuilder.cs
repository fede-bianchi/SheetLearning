using MusicApp.Application.DTOs;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.Helpers;

public static class CommentTreeBuilder
{
    public static IReadOnlyList<CommentDto> Build(
        IReadOnlyList<Comment> flat,
        Func<Comment, IReadOnlyList<CommentDto>, CommentDto> mapFn)
    {
        var byParent = flat.ToLookup(comment => comment.ParentCommentId);

        IReadOnlyList<CommentDto> BuildChildren(int? parentId)
        {
            var children = byParent[parentId]
                .OrderBy(comment => comment.CreatedAt)
                .ToList();

            return children
                .Select(comment => mapFn(comment, BuildChildren(comment.Id)))
                .ToList();
        }

        return BuildChildren(null);
    }
}

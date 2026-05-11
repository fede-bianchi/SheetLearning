using MusicApp.Domain.Entities;

namespace MusicApp.Application.Helpers;

public static class SoftDeleteContentHelper
{
    public const string DeletedPostTitolo = "[post rimosso]";
    public const string DeletedPostContenuto = "[contenuto rimosso]";
    public const string DeletedCommentContenuto = "[commento rimosso]";

    public static string ResolvePostTitolo(Post post)
        => post.IsDeleted ? DeletedPostTitolo : post.Titolo;

    public static string ResolvePostContenuto(Post post)
        => post.IsDeleted ? DeletedPostContenuto : post.Contenuto;

    public static string ResolveCommentContenuto(Comment comment)
        => comment.IsDeleted ? DeletedCommentContenuto : comment.Contenuto;
}

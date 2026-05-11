namespace MusicApp.Application.DTOs;

public record CommentDto(
    int Id,
    int PostId,
    int? ParentCommentId,
    string Contenuto,
    PublicUserDto Autore,
    int UpvoteCount,
    int DownvoteCount,
    string? UserVote,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<CommentDto> Risposte
);

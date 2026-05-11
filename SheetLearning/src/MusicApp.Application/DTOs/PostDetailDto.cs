namespace MusicApp.Application.DTOs;

public record PostDetailDto(
    int Id,
    string Titolo,
    string Contenuto,
    PublicUserDto Autore,
    int UpvoteCount,
    int DownvoteCount,
    string? UserVote,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<CommentDto> CommentiTopLevel
);

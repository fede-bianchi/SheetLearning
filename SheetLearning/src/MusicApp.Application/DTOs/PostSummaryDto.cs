namespace MusicApp.Application.DTOs;

public record PostSummaryDto(
    int Id,
    string Titolo,
    string Contenuto,
    PublicUserDto Autore,
    int UpvoteCount,
    int DownvoteCount,
    string? UserVote,
    int CommentCount,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

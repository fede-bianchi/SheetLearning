namespace MusicApp.Application.DTOs;

public record CreateCommentRequest(
    string Contenuto,
    int? ParentCommentId
);

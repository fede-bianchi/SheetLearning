namespace MusicApp.Application.DTOs;

public record TeacherSummaryDto(
    int      Id,
    int      UserId,
    string   Nickname,
    string?  Strumento,
    string?  Bio,
    string?  Specializzazioni,
    string   VisibileA,
    string[] Categorie
);

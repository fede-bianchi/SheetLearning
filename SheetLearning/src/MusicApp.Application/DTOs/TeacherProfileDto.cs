namespace MusicApp.Application.DTOs;

public record TeacherProfileDto(
    int      Id,
    int      UserId,
    string   Nickname,
    string?  Strumento,
    string?  Descrizione,
    string?  Bio,
    string?  Specializzazioni,
    string   VisibileA,
    string[] Categorie,
    DateTime CreatedAt
);

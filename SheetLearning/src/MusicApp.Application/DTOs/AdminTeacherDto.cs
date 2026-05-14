namespace MusicApp.Application.DTOs;

public record AdminTeacherDto(
    int      TeacherProfileId,
    int      UserId,
    string   Nickname,
    string   Nome,
    string   Cognome,
    string?  Strumento,
    string   VisibileA,
    string[] Categorie,
    int      TotalePrenotazioni,
    int      PrenotazioniCompletate,
    decimal  MediaValutazione,
    int      TotaleValutazioni
);

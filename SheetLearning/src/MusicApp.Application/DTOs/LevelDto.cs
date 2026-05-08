namespace MusicApp.Application.DTOs;

public record LevelDto(
    int Id,
    int ExerciseTypeId,
    int? ClefId,
    byte NumeroLivello,
    string Nome,
    string? Descrizione,
    int PunteggioMinimoSblocco
);

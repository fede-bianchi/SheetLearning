namespace MusicApp.Application.DTOs;

public record LevelDetailDto(
    int Id,
    int ExerciseTypeId,
    int? ClefId,
    byte NumeroLivello,
    string Nome,
    string? Descrizione,
    int PunteggioMinimoSblocco,
    bool Sbloccato,
    bool Completato,
    DateTime? DataSblocco,
    DateTime? DataCompletamento
);

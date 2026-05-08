namespace MusicApp.Application.DTOs;

public record AttemptDto(
    int Id,
    int ExerciseTypeId,
    string NomeEsercizio,
    int? LevelId,
    byte? Difficolta,
    int Punteggio,
    int? TempoRispostaMs,
    string? ExerciseSubtype,
    int? ToleranceWindowMs,
    string InputSource,
    DateTime CreatedAt,
    IReadOnlyList<AttemptErrorDto> Errori
);

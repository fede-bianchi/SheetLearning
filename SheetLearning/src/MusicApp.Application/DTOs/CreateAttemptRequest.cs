namespace MusicApp.Application.DTOs;

public record CreateAttemptRequest(
    int ExerciseTypeId,
    int? LevelId,
    byte? Difficolta,
    int Punteggio,
    int? TempoRispostaMs,
    string? ExerciseSubtype,
    int? ToleranceWindowMs,
    AttemptErrorDto[]? Errori,
    string InputSource
);

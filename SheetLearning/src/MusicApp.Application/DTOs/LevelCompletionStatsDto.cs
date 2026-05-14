namespace MusicApp.Application.DTOs;

public record LevelCompletionStatsDto(
    int     LevelId,
    string  LevelNome,
    int     ExerciseTypeId,
    string  ExerciseTypeNome,
    byte    NumeroLivello,
    int     PunteggioMinimoSblocco,
    int     TotaleAttemptsPerLevel,
    decimal MediaPunteggio,
    decimal PassRate,
    decimal SuggestedThreshold
);

namespace MusicApp.Application.DTOs;

public record BestScoreDto(
    int ExerciseTypeId,
    string NomeEsercizio,
    int PunteggioMigliore,
    int AttemptId,
    DateTime UpdatedAt
);

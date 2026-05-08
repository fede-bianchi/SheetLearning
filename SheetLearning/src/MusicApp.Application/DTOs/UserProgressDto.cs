namespace MusicApp.Application.DTOs;

public record UserProgressDto(
    IReadOnlyList<ExerciseProgressDto> EserciziProgress
);

public record ExerciseProgressDto(
    int ExerciseTypeId,
    string NomeEsercizio,
    int? PunteggioMigliore,
    IReadOnlyList<LevelWithProgressDto> Livelli
);

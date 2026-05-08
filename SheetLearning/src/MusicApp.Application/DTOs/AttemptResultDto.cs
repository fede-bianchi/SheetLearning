namespace MusicApp.Application.DTOs;

public record AttemptResultDto(
    int AttemptId,
    int Punteggio,
    bool IsNewRecord,
    LevelDto? LivelloSbloccato,
    int? PunteggioMinimoSuccessivo
);

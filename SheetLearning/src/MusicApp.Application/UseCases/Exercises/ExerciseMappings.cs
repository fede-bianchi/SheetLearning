using MusicApp.Application.DTOs;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Exercises;

internal static class ExerciseMappings
{
    public static ExerciseTypeDto ToExerciseTypeDto(this ExerciseType exerciseType)
    {
        return new ExerciseTypeDto(
            exerciseType.Id,
            exerciseType.Nome,
            exerciseType.Descrizione);
    }

    public static LevelDto ToLevelDto(this Level level)
    {
        return new LevelDto(
            level.Id,
            level.ExerciseTypeId,
            level.ClefId,
            level.NumeroLivello,
            level.Nome,
            level.Descrizione,
            level.PunteggioMinimoSblocco);
    }

    public static LevelWithProgressDto ToLevelWithProgressDto(this Level level, UserLevelProgress? progress)
    {
        var sbloccato = progress?.Sbloccato ?? level.NumeroLivello == 1;
        var completato = progress?.Completato ?? false;

        return new LevelWithProgressDto(
            level.Id,
            level.ExerciseTypeId,
            level.ClefId,
            level.NumeroLivello,
            level.Nome,
            level.Descrizione,
            level.PunteggioMinimoSblocco,
            sbloccato,
            completato,
            progress?.DataSblocco,
            progress?.DataCompletamento);
    }

    public static LevelDetailDto ToLevelDetailDto(this Level level, UserLevelProgress? progress)
    {
        var sbloccato = progress?.Sbloccato ?? level.NumeroLivello == 1;
        var completato = progress?.Completato ?? false;

        return new LevelDetailDto(
            level.Id,
            level.ExerciseTypeId,
            level.ClefId,
            level.NumeroLivello,
            level.Nome,
            level.Descrizione,
            level.PunteggioMinimoSblocco,
            sbloccato,
            completato,
            progress?.DataSblocco,
            progress?.DataCompletamento);
    }

    public static BestScoreDto ToBestScoreDto(this BestScore bestScore, string nomeEsercizio)
    {
        return new BestScoreDto(
            bestScore.ExerciseTypeId,
            nomeEsercizio,
            bestScore.PunteggioMigliore,
            bestScore.AttemptId,
            bestScore.UpdatedAt);
    }

    public static AttemptErrorDto ToAttemptErrorDto(this AttemptError error)
    {
        return new AttemptErrorDto(
            error.ElementType,
            error.RispostaData,
            error.RispostaCorretta,
            error.PosizioneNelPattern);
    }

    public static AttemptDto ToAttemptDto(this Attempt attempt, string nomeEsercizio)
    {
        var errori = attempt.AttemptErrors
            .OrderBy(e => e.CreatedAt)
            .ThenBy(e => e.Id)
            .Select(e => e.ToAttemptErrorDto())
            .ToList();

        return new AttemptDto(
            attempt.Id,
            attempt.ExerciseTypeId,
            nomeEsercizio,
            attempt.LevelId,
            attempt.Difficolta,
            attempt.Punteggio,
            attempt.TempoRispostaMs,
            attempt.ExerciseSubtype,
            attempt.ToleranceWindowMs,
            attempt.InputSource,
            attempt.CreatedAt,
            errori);
    }
}

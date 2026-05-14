using MusicApp.Application.Caching;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Exercises;

public class SaveAttemptHandler
{
    private static readonly string[] RitmoSubtypes = ["riconoscimento", "esecuzione"];

    private readonly IExerciseTypeRepository _exerciseTypeRepository;
    private readonly ILevelRepository _levelRepository;
    private readonly IAttemptRepository _attemptRepository;
    private readonly IBestScoreRepository _bestScoreRepository;
    private readonly IUserLevelProgressRepository _userLevelProgressRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ICacheService _cache;

    public SaveAttemptHandler(
        IExerciseTypeRepository exerciseTypeRepository,
        ILevelRepository levelRepository,
        IAttemptRepository attemptRepository,
        IBestScoreRepository bestScoreRepository,
        IUserLevelProgressRepository userLevelProgressRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        ICacheService cache)
    {
        _exerciseTypeRepository = exerciseTypeRepository;
        _levelRepository = levelRepository;
        _attemptRepository = attemptRepository;
        _bestScoreRepository = bestScoreRepository;
        _userLevelProgressRepository = userLevelProgressRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _cache = cache;
    }

    public async Task<Result<AttemptResultDto>> HandleAsync(int userId, CreateAttemptRequest request)
    {
        var exerciseType = await _exerciseTypeRepository.GetByIdAsync(request.ExerciseTypeId);
        if (exerciseType is null)
        {
            return Result<AttemptResultDto>.Fail(
                ErrorCodes.ExerciseTypeNotFound,
                "Tipo esercizio non trovato.");
        }

        if (request.Punteggio is < 0 or > 100)
        {
            return Result<AttemptResultDto>.Fail(
                ErrorCodes.InvalidPunteggio,
                "Punteggio non valido. Il valore deve essere compreso tra 0 e 100.");
        }

        var levelsForType = await _levelRepository.GetByExerciseTypeAsync(request.ExerciseTypeId);
        var isLevelBasedExercise = levelsForType.Count > 0;
        Level? level = null;

        if (isLevelBasedExercise)
        {
            if (!ValidateLevelBasedRequest(request, out var validationError))
            {
                return validationError!;
            }

            level = await _levelRepository.GetByIdAsync(request.LevelId!.Value);
            if (level is null)
            {
                return Result<AttemptResultDto>.Fail(
                    ErrorCodes.LevelNotFound,
                    "Livello non trovato.");
            }

            if (level.ExerciseTypeId != request.ExerciseTypeId)
            {
                return Result<AttemptResultDto>.Fail(
                    ErrorCodes.LevelExerciseMismatch,
                    "Il livello selezionato non appartiene al tipo esercizio indicato.");
            }
        }
        else
        {
            if (!ValidateRitmoRequest(request, out var validationError))
            {
                return validationError!;
            }
        }

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var now = DateTime.UtcNow;
            var attempt = new Attempt
            {
                UserId = userId,
                ExerciseTypeId = request.ExerciseTypeId,
                LevelId = request.LevelId,
                Difficolta = request.Difficolta,
                Punteggio = request.Punteggio,
                TempoRispostaMs = request.TempoRispostaMs,
                ExerciseSubtype = request.ExerciseSubtype,
                ToleranceWindowMs = request.ToleranceWindowMs,
                InputSource = request.InputSource,
                CreatedAt = now
            };

            if (request.Errori is { Length: > 0 })
            {
                foreach (var errore in request.Errori)
                {
                    attempt.AttemptErrors.Add(new AttemptError
                    {
                        ElementType = errore.ElementType,
                        RispostaData = errore.RispostaData,
                        RispostaCorretta = errore.RispostaCorretta,
                        PosizioneNelPattern = errore.PosizioneNelPattern,
                        CreatedAt = now
                    });
                }
            }

            attempt = await _attemptRepository.CreateAsync(attempt);

            var bestScore = await _bestScoreRepository.GetByUserAndTypeAsync(userId, request.ExerciseTypeId);
            var isNewRecord = false;

            if (bestScore is null)
            {
                await _bestScoreRepository.CreateAsync(new BestScore
                {
                    UserId = userId,
                    ExerciseTypeId = request.ExerciseTypeId,
                    PunteggioMigliore = attempt.Punteggio,
                    AttemptId = attempt.Id
                });
                isNewRecord = true;
            }
            else if (attempt.Punteggio > bestScore.PunteggioMigliore)
            {
                bestScore.PunteggioMigliore = attempt.Punteggio;
                bestScore.AttemptId = attempt.Id;
                await _bestScoreRepository.UpdateAsync(bestScore);
                isNewRecord = true;
            }

            // Phase 7 — Notification dispatch
            if (isNewRecord)
            {
                _ = _notificationService.SendRecordBattuoAsync(
                    userId,
                    exerciseType.Nome,
                    attempt.Punteggio);
            }

            LevelDto? livelloSbloccato = null;
            int? punteggioMinimoSuccessivo = null;

            if (level is not null)
            {
                await _userLevelProgressRepository.UpsertAsync(new UserLevelProgress
                {
                    UserId = userId,
                    LevelId = level.Id,
                    Sbloccato = true,
                    Completato = attempt.Punteggio >= level.PunteggioMinimoSblocco,
                    DataSblocco = now,
                    DataCompletamento = attempt.Punteggio >= level.PunteggioMinimoSblocco ? now : null
                });

                var nextLevel = await _levelRepository.GetNextLevelAsync(request.ExerciseTypeId, level.NumeroLivello);
                punteggioMinimoSuccessivo = nextLevel?.PunteggioMinimoSblocco;

                if (attempt.Punteggio >= level.PunteggioMinimoSblocco && nextLevel is not null)
                {
                    var existingProgress = await _userLevelProgressRepository.GetAsync(userId, nextLevel.Id);
                    if (existingProgress is null || !existingProgress.Sbloccato)
                    {
                        await _userLevelProgressRepository.UpsertAsync(new UserLevelProgress
                        {
                            UserId = userId,
                            LevelId = nextLevel.Id,
                            Sbloccato = true,
                            Completato = false,
                            DataSblocco = now
                        });

                        livelloSbloccato = nextLevel.ToLevelDto();

                        // Phase 7 — Notification dispatch
                        _ = _notificationService.SendLivelloSbloccatoAsync(
                            userId,
                            nextLevel.Nome,
                            exerciseType.Nome,
                            nextLevel.Id);

                        // Phase 9 — Cache invalidation
                        _cache.Invalidate(CacheKeys.UserLevels(userId));
                    }
                }
            }

            var count = await _attemptRepository.CountByUserAndTypeAsync(userId, request.ExerciseTypeId);
            if (count > 50)
            {
                var currentBest = await _bestScoreRepository.GetByUserAndTypeAsync(userId, request.ExerciseTypeId);
                var protectedIds = currentBest is not null
                    ? new[] { currentBest.AttemptId }
                    : Array.Empty<int>();

                var toDelete = await _attemptRepository.GetOldestDeletableIdsAsync(
                    userId,
                    request.ExerciseTypeId,
                    protectedIds,
                    count - 50);

                if (toDelete.Count > 0)
                {
                    await _attemptRepository.DeleteByIdsAsync(toDelete);
                }
            }

            await _unitOfWork.CommitTransactionAsync();

            return Result<AttemptResultDto>.Ok(new AttemptResultDto(
                AttemptId: attempt.Id,
                Punteggio: attempt.Punteggio,
                IsNewRecord: isNewRecord,
                LivelloSbloccato: livelloSbloccato,
                PunteggioMinimoSuccessivo: punteggioMinimoSuccessivo));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static bool ValidateLevelBasedRequest(CreateAttemptRequest request, out Result<AttemptResultDto>? error)
    {
        if (request.LevelId is null)
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.LevelNotFound,
                "Il livello e obbligatorio per questo tipo esercizio.");
            return false;
        }

        if (request.Difficolta is not null)
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.InvalidDifficolta,
                "Difficolta deve essere null per esercizi basati su livelli.");
            return false;
        }

        if (request.ExerciseSubtype is not null)
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.LevelExerciseMismatch,
                "ExerciseSubtype deve essere null per esercizi basati su livelli.");
            return false;
        }

        if (request.ToleranceWindowMs is not null)
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.LevelExerciseMismatch,
                "ToleranceWindowMs deve essere null per esercizi basati su livelli.");
            return false;
        }

        error = null;
        return true;
    }

    private static bool ValidateRitmoRequest(CreateAttemptRequest request, out Result<AttemptResultDto>? error)
    {
        if (request.LevelId is not null)
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.LevelExerciseMismatch,
                "LevelId deve essere null per Ritmo.");
            return false;
        }

        if (request.Difficolta is null)
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.InvalidDifficolta,
                "Difficolta e obbligatoria per Ritmo.");
            return false;
        }

        if (request.Difficolta is < 0 or > 30)
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.InvalidDifficolta,
                "Difficolta deve essere compresa tra 0 e 30.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(request.ExerciseSubtype) || !RitmoSubtypes.Contains(request.ExerciseSubtype))
        {
            error = Result<AttemptResultDto>.Fail(
                ErrorCodes.LevelExerciseMismatch,
                "ExerciseSubtype deve essere 'riconoscimento' o 'esecuzione' per Ritmo.");
            return false;
        }

        if (request.ExerciseSubtype == "esecuzione")
        {
            if (request.ToleranceWindowMs is null)
            {
                error = Result<AttemptResultDto>.Fail(
                    ErrorCodes.LevelExerciseMismatch,
                    "ToleranceWindowMs e obbligatorio per subtype 'esecuzione'.");
                return false;
            }

            if (request.ToleranceWindowMs is not (150 or 100 or 50))
            {
                error = Result<AttemptResultDto>.Fail(
                    ErrorCodes.LevelExerciseMismatch,
                    "ToleranceWindowMs deve essere 150, 100 o 50.");
                return false;
            }
        }

        error = null;
        return true;
    }
}

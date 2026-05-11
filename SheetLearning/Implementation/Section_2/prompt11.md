# SYSTEM PROMPT — Phase 2: Exercises, Progression & Attempts
# Music Learning App

You are a senior .NET backend engineer continuing work on a solution built
across Phase 0 (infrastructure + database) and Phase 1 (authentication +
user profile). Both phases are complete and must not be modified.

Your scope in this phase:
- Application layer: new repository interfaces, use-case handlers, DTOs,
  FluentValidation validators for the 8 endpoints listed below
- Infrastructure layer: implementations of all new repository interfaces
- Web layer: 3 new API controllers, extension of UsersController with 3 new
  endpoints; no new authorization policies (all endpoints use DefaultPolicy
  or are anonymous)
- Program.cs: register all new services introduced in this phase only

No frontend, no Razor Pages, no JavaScript libraries. Backend API only.

---

## SECTION 1 — PREREQUISITES & CONSTRAINTS

**Phases 0 and 1 outputs (already exist, do not touch):**
- All 22 EF Core entities and configurations in Domain + Infrastructure
- `AppDbContext`, migrations, `DbSeeder`
- `IUserRepository`, `ISessionRepository`, `IPasswordHasher`, `IJwtService`,
  `IRefreshCookieHelper` and their implementations
- All auth and user-profile handlers, validators, DTOs from Phase 1
- `AuthController`, `UsersController` (with Phase 1 endpoints only)
- `Result<T>`, `ErrorCodes`, `ApiError`, `JwtOptions`
- `DefaultPolicy` (RequireAuthenticatedUser + is_active = "True") and
  `CanDeleteOwnAccount` registered in `Program.cs`

**Architecture rules (identical to Phase 1):**
- `MusicApp.Domain` — entities only, already complete, do not modify
- `MusicApp.Application` — interfaces, handlers, DTOs, validators. No EF Core.
- `MusicApp.Infrastructure` — implements Application interfaces via EF Core.
- `MusicApp.Web` — controllers, DI wiring only.

**Relevant seeded data (read-only, do not re-seed):**
- `exercise_types`: id=1 "Lettura Note", id=2 "Lettura Accordi", id=3 "Ritmo"
- `clefs`: id=1 "Chiave di Sol", id=2 "Chiave di Basso"
- `levels` id=1..3 for Lettura Note, id=4..6 for Lettura Accordi.
  Ritmo has NO rows in `levels` — it uses the `difficolta` (0–30) field
  on `attempts` instead.

**New ErrorCodes to add** to `MusicApp.Application/Common/Result.cs`
(append to the existing `ErrorCodes` static class — do not replace it):

```csharp
// Phase 2
public const string ExerciseTypeNotFound  = "EXERCISE_TYPE_NOT_FOUND";
public const string LevelNotFound         = "LEVEL_NOT_FOUND";
public const string LevelExerciseMismatch = "LEVEL_EXERCISE_MISMATCH";
public const string AttemptNotFound       = "ATTEMPT_NOT_FOUND";
public const string InvalidDifficolta     = "INVALID_DIFFICOLTA";
public const string InvalidPunteggio      = "INVALID_PUNTEGGIO";
```

---

## SECTION 2 — DOMAIN RULES (business invariants)

These rules govern the core exercise and progression logic. Every handler
must enforce them without exception.

### 2.1 Exercise type / level mutual exclusivity

- **Lettura Note (id=1) and Lettura Accordi (id=2):** use levels.
  `LevelId` MUST be provided. `Difficolta` MUST be null.
  `ExerciseSubtype` MUST be null. `ToleranceWindowMs` MUST be null.

- **Ritmo (id=3):** does NOT use levels.
  `LevelId` MUST be null. `Difficolta` MUST be provided (0–30).
  `ExerciseSubtype` MUST be one of `"riconoscimento"` / `"esecuzione"`.
  `ToleranceWindowMs` MUST be provided when subtype is `"esecuzione"`;
  valid values are 150, 100, or 50 (ms).

- **TempoRispostaMs:** meaningful only for Lettura Note. Accept it for any
  exercise type (do not reject it if sent for other types) — store as-is.

- These rules are enforced in both the FluentValidation validator (structural
  checks on the request shape) and the handler (semantic checks against DB data).

### 2.2 Level ownership check

When `LevelId` is provided, the handler must verify that
`level.ExerciseTypeId == request.ExerciseTypeId`. A client sending a valid
level id that belongs to a different exercise type must receive an error.

### 2.3 Punteggio range

`Punteggio` must be in [0, 100] inclusive. Reject outside this range.

### 2.4 Level unlock logic

After saving an attempt, if `LevelId` is provided:
1. Read `level.PunteggioMinimoSblocco` for the submitted level.
2. If `attempt.Punteggio >= level.PunteggioMinimoSblocco`:
   - Find the next level: same `ExerciseTypeId`, `NumeroLivello = level.NumeroLivello + 1`.
   - If a next level exists AND the user does NOT already have a
     `UserLevelProgress` row for it with `Sbloccato = true`:
     - Upsert a `UserLevelProgress` row: `Sbloccato = true`,
       `DataSblocco = DateTime.UtcNow`.
     - Populate `LivelloSbloccato` in the response DTO.
3. Additionally, mark the current level as completed:
   - Upsert `UserLevelProgress` for the current level:
     `Completato = true`, `DataCompletamento = DateTime.UtcNow`.
     If the row does not exist yet, also set `Sbloccato = true` for it.

### 2.5 Best score upsert

After saving an attempt:
1. Look up the `BestScore` row for `(UserId, ExerciseTypeId)`.
2. If no row exists: create one with `PunteggioMigliore = attempt.Punteggio`,
   `AttemptId = attempt.Id`.
3. If a row exists and `attempt.Punteggio > bestScore.PunteggioMigliore`:
   update `PunteggioMigliore` and `AttemptId`.
4. Set `IsNewRecord = true` in the response if and only if a new best was set.

### 2.6 Attempt rotation (max 50 per user per exercise type)

After saving a new attempt AND after the best-score upsert:
1. Count all `Attempt` rows for `(UserId, ExerciseTypeId)`.
2. If count > 50:
   - Identify the oldest attempts by `CreatedAt` ascending.
   - Exclude any attempt whose `Id` is currently referenced by a `BestScore`
     row for this user (i.e., `best_scores.attempt_id`). That row must never
     be deleted.
   - Delete oldest non-protected attempts until total count == 50.
3. This operation runs in the same database transaction as the attempt save.

### 2.7 UserLevelProgress initial state

`user_level_progress` rows are created lazily — never pre-seeded per user.
The rule for computing effective unlock state when no DB row exists:
- `NumeroLivello == 1` for any exercise type → treated as unlocked (always
  accessible). The handler returns `Sbloccato = true` even if no row exists.
- `NumeroLivello > 1` → treated as locked if no row exists.

This rule applies in `GetLevelsHandler`, `GetLevelDetailHandler`, and
`GetLevelsByExerciseTypeHandler`.

---

## SECTION 3 — APPLICATION LAYER: INTERFACES

Create in `MusicApp.Application/Interfaces/`.

### 3.1 `IExerciseTypeRepository`

```csharp
Task<IReadOnlyList<ExerciseType>> GetAllAsync();
Task<ExerciseType?> GetByIdAsync(int id);
```

### 3.2 `ILevelRepository`

```csharp
Task<IReadOnlyList<Level>> GetAllAsync();
Task<Level?> GetByIdAsync(int id);
Task<IReadOnlyList<Level>> GetByExerciseTypeAsync(int exerciseTypeId);
// Returns the level with NumeroLivello = current.NumeroLivello + 1
// for the same exercise type, or null if none exists.
Task<Level?> GetNextLevelAsync(int exerciseTypeId, byte currentNumeroLivello);
```

### 3.3 `IAttemptRepository`

```csharp
Task<Attempt> CreateAsync(Attempt attempt);
Task<IReadOnlyList<Attempt>> GetByUserAndTypeAsync(int userId, int? exerciseTypeId);
Task<int> CountByUserAndTypeAsync(int userId, int exerciseTypeId);
// Returns the oldest attempt IDs for (userId, exerciseTypeId),
// excluding protectedAttemptIds, ordered by CreatedAt ascending.
Task<IReadOnlyList<int>> GetOldestDeletableIdsAsync(
    int userId, int exerciseTypeId,
    IEnumerable<int> protectedAttemptIds,
    int excessCount);
Task DeleteByIdsAsync(IEnumerable<int> ids);
```

### 3.4 `IBestScoreRepository`

```csharp
Task<BestScore?> GetByUserAndTypeAsync(int userId, int exerciseTypeId);
Task<IReadOnlyList<BestScore>> GetAllByUserAsync(int userId);
Task<BestScore> CreateAsync(BestScore bestScore);
Task<BestScore> UpdateAsync(BestScore bestScore);
```

### 3.5 `IUserLevelProgressRepository`

```csharp
Task<UserLevelProgress?> GetAsync(int userId, int levelId);
Task<IReadOnlyList<UserLevelProgress>> GetAllByUserAsync(int userId);
Task<UserLevelProgress> UpsertAsync(UserLevelProgress progress);
```

`UpsertAsync` performs an INSERT if no row exists for `(UserId, LevelId)`,
or an UPDATE if one does. Uses EF Core `AddOrUpdate` semantics:
check existence, then create or update and call `SaveChangesAsync`.

---

## SECTION 4 — APPLICATION LAYER: DTOs

Add to `MusicApp.Application/DTOs/`. Do not modify existing DTO files.

### 4.1 `ExerciseTypeDto`

```csharp
public record ExerciseTypeDto(
    int    Id,
    string Nome,
    string? Descrizione
);
```

### 4.2 `LevelDto` (minimal, used inline in other DTOs)

```csharp
public record LevelDto(
    int    Id,
    int    ExerciseTypeId,
    int?   ClefId,
    byte   NumeroLivello,
    string Nome,
    string? Descrizione,
    int    PunteggioMinimoSblocco
);
```

### 4.3 `LevelWithProgressDto`

```csharp
public record LevelWithProgressDto(
    int    Id,
    int    ExerciseTypeId,
    int?   ClefId,
    byte   NumeroLivello,
    string Nome,
    string? Descrizione,
    int    PunteggioMinimoSblocco,
    bool   Sbloccato,     // computed per domain rule 2.7
    bool   Completato,
    DateTime? DataSblocco,
    DateTime? DataCompletamento
);
```

### 4.4 `LevelDetailDto`

Same fields as `LevelWithProgressDto`. Use a type alias or inheritance
if preferred, but both types must be independently serializable.

### 4.5 `UserProgressDto`

```csharp
public record UserProgressDto(
    IReadOnlyList<ExerciseProgressDto> EserciziProgress
);

public record ExerciseProgressDto(
    int    ExerciseTypeId,
    string NomeEsercizio,
    int?   PunteggioMigliore,    // null if no attempt yet
    IReadOnlyList<LevelWithProgressDto> Livelli  // empty for Ritmo
);
```

### 4.6 `CreateAttemptRequest`

```csharp
public record CreateAttemptRequest(
    int    ExerciseTypeId,
    int?   LevelId,
    byte?  Difficolta,
    int    Punteggio,
    int?   TempoRispostaMs,
    string? ExerciseSubtype,
    int?   ToleranceWindowMs,
    AttemptErrorDto[]? Errori,
    string InputSource
);
```

### 4.7 `AttemptErrorDto`

```csharp
public record AttemptErrorDto(
    string  ElementType,          // "nota" / "accordo" / "ritmo"
    string  RispostaData,         // what the user answered
    string  RispostaCorretta,     // the correct answer
    byte?   PosizioneNelPattern   // rhythm only: beat index of the error
);
```

### 4.8 `AttemptResultDto`

```csharp
public record AttemptResultDto(
    int      AttemptId,
    int      Punteggio,
    bool     IsNewRecord,
    LevelDto? LivelloSbloccato,         // non-null if a new level was unlocked
    int?     PunteggioMinimoSuccessivo  // threshold for unlocking the next level
);
```

`PunteggioMinimoSuccessivo` is the `PunteggioMinimoSblocco` of the NEXT
level (the one just unlocked, or the one not yet unlocked). Null if the
submitted level was the last one for that exercise type, or if the attempt
was for Ritmo (no levels).

### 4.9 `BestScoreDto`

```csharp
public record BestScoreDto(
    int      ExerciseTypeId,
    string   NomeEsercizio,
    int      PunteggioMigliore,
    int      AttemptId,
    DateTime UpdatedAt
);
```

### 4.10 `AttemptDto`

```csharp
public record AttemptDto(
    int      Id,
    int      ExerciseTypeId,
    string   NomeEsercizio,
    int?     LevelId,
    byte?    Difficolta,
    int      Punteggio,
    int?     TempoRispostaMs,
    string?  ExerciseSubtype,
    int?     ToleranceWindowMs,
    string   InputSource,
    DateTime CreatedAt,
    IReadOnlyList<AttemptErrorDto> Errori
);
```

### 4.11 `AttemptQuery` (query string parameters)

```csharp
public record AttemptQuery(
    int? ExerciseTypeId  // optional filter; null = return attempts for all types
);
```

---

## SECTION 5 — APPLICATION LAYER: VALIDATORS

Add to `MusicApp.Application/Validators/`. Register automatically via
the existing `AddValidatorsFromAssemblyContaining` call in
`MusicApp.Application/DependencyInjection.cs`.

### 5.1 `CreateAttemptRequestValidator`

| Rule | Detail |
|---|---|
| `ExerciseTypeId` | `GreaterThan(0)` |
| `Punteggio` | `InclusiveBetween(0, 100)` |
| `InputSource` | `NotEmpty`, must be one of `"mouse"`, `"keyboard"`, `"midi"` |
| `Difficolta` | When not null: `InclusiveBetween(0, 30)` |
| `ExerciseSubtype` | When not null: must be `"riconoscimento"` or `"esecuzione"` |
| `ToleranceWindowMs` | When not null: must be 50, 100, or 150 |
| Cross-field: LevelId/Difficolta | Cannot both be non-null simultaneously |
| `Errori[*].ElementType` | When array not null: each must be `"nota"`, `"accordo"`, or `"ritmo"` |
| `Errori[*].RispostaData` | `NotEmpty`, `MaxLength(100)` |
| `Errori[*].RispostaCorretta` | `NotEmpty`, `MaxLength(100)` |

Semantic cross-field rules (exercise type vs level/difficulty) are enforced
in the handler after the ExerciseType is loaded from DB, not in the validator.
The validator only performs structural/format checks.

---

## SECTION 6 — APPLICATION LAYER: USE-CASE HANDLERS

All handlers live in `MusicApp.Application/UseCases/Exercises/`.
Each receives only interface dependencies via constructor injection.

### 6.1 `GetExerciseTypesHandler`

Dependencies: `IExerciseTypeRepository`

Logic:
1. Call `GetAllAsync()`.
2. Map each to `ExerciseTypeDto`.
3. Return `Result.Ok(dtos)`.

### 6.2 `GetLevelsHandler`

Dependencies: `ILevelRepository`, `IUserLevelProgressRepository`

Logic:
1. Load all levels via `ILevelRepository.GetAllAsync()`.
2. Load all progress rows for the user via
   `IUserLevelProgressRepository.GetAllByUserAsync(userId)`.
3. For each level, compute `Sbloccato` and `Completato`:
   - Find the matching `UserLevelProgress` row by `LevelId`.
   - If no row exists: `Sbloccato = (level.NumeroLivello == 1)`,
     `Completato = false`, timestamps null.
   - If row exists: use its values.
4. Map to `LevelWithProgressDto[]` and return `Result.Ok(dtos)`.

### 6.3 `GetLevelDetailHandler`

Dependencies: `ILevelRepository`, `IUserLevelProgressRepository`

Logic:
1. Load level by id. If null, return `Result.Fail(ErrorCodes.LevelNotFound, "...")`.
2. Load progress row for `(userId, levelId)`. Apply same rule as 6.2 step 3.
3. Return `Result.Ok(dto)`.

### 6.4 `GetLevelsByExerciseTypeHandler`

Dependencies: `ILevelRepository`, `IUserLevelProgressRepository`,
`IExerciseTypeRepository`

Logic:
1. Verify exercise type exists. If null, return
   `Result.Fail(ErrorCodes.ExerciseTypeNotFound, "...")`.
2. Load levels by exercise type via `GetByExerciseTypeAsync`.
3. Load all progress rows for the user.
4. Map as in 6.2 steps 3–4.
5. Return `Result.Ok(dtos)`.

### 6.5 `GetMyProgressHandler`

Dependencies: `IExerciseTypeRepository`, `ILevelRepository`,
`IUserLevelProgressRepository`, `IBestScoreRepository`

Logic:
1. Load all exercise types.
2. Load all levels.
3. Load all progress rows for the user.
4. Load all best scores for the user.
5. For each exercise type, build `ExerciseProgressDto`:
   - `PunteggioMigliore`: from best score row, null if none.
   - `Livelli`: filtered from all levels where
     `level.ExerciseTypeId == exerciseType.Id`, with progress computed
     per domain rule 2.7. Empty list for Ritmo (it has no levels).
6. Return `Result.Ok(new UserProgressDto(...))`.

### 6.6 `SaveAttemptHandler`

File: `MusicApp.Application/UseCases/Exercises/SaveAttemptHandler.cs`

This is the most complex handler in this phase. It must execute all steps
within a single database transaction. Use `IDbTransaction` or wrap in
a unit-of-work pattern by injecting `AppDbContext` directly into the
Infrastructure implementation — the handler itself depends only on
repository interfaces.

Dependencies: `IExerciseTypeRepository`, `ILevelRepository`,
`IAttemptRepository`, `IBestScoreRepository`,
`IUserLevelProgressRepository`

**Step-by-step logic:**

**Step 1 — Validate exercise type:**
Load exercise type by `request.ExerciseTypeId`. If null, return
`Result.Fail(ErrorCodes.ExerciseTypeNotFound, "...")`.

**Step 2 — Validate level/difficulty cross-field semantics:**
Determine if this exercise type uses levels by checking whether any
levels exist for it via `ILevelRepository.GetByExerciseTypeAsync`.
- If exercise type HAS levels (Lettura Note, Lettura Accordi):
  - `LevelId` must not be null → if null, return
    `Result.Fail(ErrorCodes.LevelNotFound, "Level is required for this exercise type.")`.
  - `Difficolta` must be null → if not null, return
    `Result.Fail(ErrorCodes.InvalidDifficolta, "Difficolta must be null for level-based exercises.")`.
  - Load the level by `LevelId`. If null, return
    `Result.Fail(ErrorCodes.LevelNotFound, "...")`.
  - Verify `level.ExerciseTypeId == request.ExerciseTypeId`. If not,
    return `Result.Fail(ErrorCodes.LevelExerciseMismatch, "...")`.
- If exercise type has NO levels (Ritmo):
  - `LevelId` must be null → if not null, return
    `Result.Fail(ErrorCodes.LevelExerciseMismatch, "LevelId must be null for Ritmo.")`.
  - `Difficolta` must not be null → if null, return
    `Result.Fail(ErrorCodes.InvalidDifficolta, "Difficolta is required for Ritmo.")`.

**Step 3 — Save the Attempt entity:**
```csharp
var attempt = new Attempt
{
    UserId           = userId,
    ExerciseTypeId   = request.ExerciseTypeId,
    LevelId          = request.LevelId,
    Difficolta       = request.Difficolta,
    Punteggio        = request.Punteggio,
    TempoRispostaMs  = request.TempoRispostaMs,
    ExerciseSubtype  = request.ExerciseSubtype,
    ToleranceWindowMs = request.ToleranceWindowMs,
    InputSource      = request.InputSource,
    CreatedAt        = DateTime.UtcNow
};
attempt = await attemptRepo.CreateAsync(attempt);
```

**Step 4 — Save AttemptError rows (bulk):**
If `request.Errori` is not null and not empty, create one `AttemptError`
entity per item:
```csharp
foreach (var e in request.Errori)
{
    var error = new AttemptError
    {
        AttemptId           = attempt.Id,
        ElementType         = e.ElementType,
        RispostaData        = e.RispostaData,
        RispostaCorretta    = e.RispostaCorretta,
        PosizioneNelPattern = e.PosizioneNelPattern,
        CreatedAt           = DateTime.UtcNow
    };
    // add to context — bulk save via SaveChangesAsync at end of transaction
}
```
Save all error rows in a single `SaveChangesAsync` call, not one per row.

**Step 5 — Best score upsert (domain rule 2.5):**
```csharp
var bestScore = await bestScoreRepo.GetByUserAndTypeAsync(userId, request.ExerciseTypeId);
bool isNewRecord = false;

if (bestScore == null)
{
    await bestScoreRepo.CreateAsync(new BestScore
    {
        UserId             = userId,
        ExerciseTypeId     = request.ExerciseTypeId,
        PunteggioMigliore  = attempt.Punteggio,
        AttemptId          = attempt.Id
    });
    isNewRecord = true;
}
else if (attempt.Punteggio > bestScore.PunteggioMigliore)
{
    bestScore.PunteggioMigliore = attempt.Punteggio;
    bestScore.AttemptId         = attempt.Id;
    await bestScoreRepo.UpdateAsync(bestScore);
    isNewRecord = true;
}
```

**Step 6 — Level unlock (domain rule 2.4):**
Only execute this block if a level was loaded in Step 2 (i.e., `level != null`).

```csharp
LevelDto? livelloSbloccato = null;
int? punteggioMinimoSuccessivo = null;

// Mark current level as completed
await userLevelProgressRepo.UpsertAsync(new UserLevelProgress
{
    UserId             = userId,
    LevelId            = level.Id,
    Sbloccato          = true,
    Completato         = attempt.Punteggio >= level.PunteggioMinimoSblocco,
    DataSblocco        = DateTime.UtcNow,
    DataCompletamento  = attempt.Punteggio >= level.PunteggioMinimoSblocco
                         ? DateTime.UtcNow : null
});

if (attempt.Punteggio >= level.PunteggioMinimoSblocco)
{
    var nextLevel = await levelRepo.GetNextLevelAsync(
        request.ExerciseTypeId, level.NumeroLivello);

    if (nextLevel != null)
    {
        var existing = await userLevelProgressRepo.GetAsync(userId, nextLevel.Id);
        if (existing == null || !existing.Sbloccato)
        {
            await userLevelProgressRepo.UpsertAsync(new UserLevelProgress
            {
                UserId      = userId,
                LevelId     = nextLevel.Id,
                Sbloccato   = true,
                Completato  = false,
                DataSblocco = DateTime.UtcNow
            });
            livelloSbloccato = MapToLevelDto(nextLevel);
        }
        punteggioMinimoSuccessivo = nextLevel.PunteggioMinimoSblocco;
    }
}
```

**Step 7 — Attempt rotation (domain rule 2.6):**
```csharp
var count = await attemptRepo.CountByUserAndTypeAsync(userId, request.ExerciseTypeId);
if (count > 50)
{
    // Protect the current best score's attempt_id
    var currentBest = await bestScoreRepo.GetByUserAndTypeAsync(
        userId, request.ExerciseTypeId);
    var protectedIds = currentBest != null
        ? new[] { currentBest.AttemptId }
        : Array.Empty<int>();

    var toDelete = await attemptRepo.GetOldestDeletableIdsAsync(
        userId, request.ExerciseTypeId,
        protectedIds,
        excessCount: count - 50);

    if (toDelete.Count > 0)
        await attemptRepo.DeleteByIdsAsync(toDelete);
}
```

**Step 8 — Return result:**
```csharp
return Result.Ok(new AttemptResultDto(
    AttemptId                : attempt.Id,
    Punteggio                : attempt.Punteggio,
    IsNewRecord              : isNewRecord,
    LivelloSbloccato         : livelloSbloccato,
    PunteggioMinimoSuccessivo: punteggioMinimoSuccessivo
));
```

All database writes in steps 3–7 must execute within a single transaction.
If any step fails after step 3, the entire operation must be rolled back.
Implement the transaction at the Infrastructure layer by having the
repositories share the same `AppDbContext` scope (which ASP.NET DI ensures
when all repositories are `Scoped`) and calling `SaveChangesAsync` once at
the very end of the operation. To enable this pattern, add a
`IUnitOfWork.CommitAsync()` abstraction or simply call `context.SaveChangesAsync()`
once after all entity mutations. Choose the simplest approach that guarantees
atomicity.

### 6.7 `GetMyBestScoresHandler`

Dependencies: `IBestScoreRepository`, `IExerciseTypeRepository`

Logic:
1. Load all best score rows for the user.
2. Load all exercise types (for the name).
3. Map each `BestScore` to `BestScoreDto`.
4. Return `Result.Ok(dtos)`.

### 6.8 `GetMyAttemptsHandler`

Dependencies: `IAttemptRepository`, `IExerciseTypeRepository`

Logic:
1. Load attempts via `IAttemptRepository.GetByUserAndTypeAsync(userId, query.ExerciseTypeId)`.
   This returns at most 50 per type (rotation guarantees this).
2. Load exercise types for the name mapping.
3. For each attempt, load its `AttemptErrors` (must be included in the
   repository query via `.Include(a => a.AttemptErrors)`).
4. Map to `AttemptDto[]`.
5. Return `Result.Ok(dtos)`.

---

## SECTION 7 — INFRASTRUCTURE LAYER: IMPLEMENTATIONS

All repository implementations live in
`MusicApp.Infrastructure/Repositories/`. Each injects `AppDbContext`.

### 7.1 `ExerciseTypeRepository`

`GetAllAsync`: `context.ExerciseTypes.AsNoTracking().ToListAsync()`
`GetByIdAsync`: `FindAsync(id)` or `FirstOrDefaultAsync`

### 7.2 `LevelRepository`

`GetAllAsync`: `.AsNoTracking().ToListAsync()`
`GetByIdAsync`: standard
`GetByExerciseTypeAsync`:
```csharp
context.Levels
    .Where(l => l.ExerciseTypeId == exerciseTypeId)
    .OrderBy(l => l.NumeroLivello)
    .AsNoTracking()
    .ToListAsync()
```
`GetNextLevelAsync`:
```csharp
context.Levels
    .Where(l => l.ExerciseTypeId == exerciseTypeId
             && l.NumeroLivello == currentNumeroLivello + 1)
    .AsNoTracking()
    .FirstOrDefaultAsync()
```

### 7.3 `AttemptRepository`

`CreateAsync`: add entity, `SaveChangesAsync`, return saved entity.

`GetByUserAndTypeAsync`:
```csharp
var query = context.Attempts
    .Include(a => a.AttemptErrors)
    .Where(a => a.UserId == userId);
if (exerciseTypeId.HasValue)
    query = query.Where(a => a.ExerciseTypeId == exerciseTypeId.Value);
return await query
    .OrderByDescending(a => a.CreatedAt)
    .AsNoTracking()
    .ToListAsync();
```

`CountByUserAndTypeAsync`:
```csharp
context.Attempts
    .CountAsync(a => a.UserId == userId && a.ExerciseTypeId == exerciseTypeId)
```

`GetOldestDeletableIdsAsync`:
```csharp
context.Attempts
    .Where(a => a.UserId == userId
             && a.ExerciseTypeId == exerciseTypeId
             && !protectedAttemptIds.Contains(a.Id))
    .OrderBy(a => a.CreatedAt)
    .Take(excessCount)
    .Select(a => a.Id)
    .ToListAsync()
```

`DeleteByIdsAsync`:
```csharp
await context.Attempts
    .Where(a => ids.Contains(a.Id))
    .ExecuteDeleteAsync();  // EF Core 7+ bulk delete — no round-trip
```

Note: `ExecuteDeleteAsync` does not trigger EF Core cascade tracking.
`attempt_errors` has `DeleteBehavior.Cascade` at the DB level (configured
in Phase 0), so MariaDB will cascade the delete automatically.

### 7.4 `BestScoreRepository`

`GetByUserAndTypeAsync`:
```csharp
context.BestScores
    .FirstOrDefaultAsync(b => b.UserId == userId
                           && b.ExerciseTypeId == exerciseTypeId)
```

`GetAllByUserAsync`:
```csharp
context.BestScores
    .Where(b => b.UserId == userId)
    .AsNoTracking()
    .ToListAsync()
```

`CreateAsync` and `UpdateAsync`: standard add/update + `SaveChangesAsync`.

### 7.5 `UserLevelProgressRepository`

`GetAsync`:
```csharp
context.UserLevelProgresses
    .FirstOrDefaultAsync(p => p.UserId == userId && p.LevelId == levelId)
```

`GetAllByUserAsync`:
```csharp
context.UserLevelProgresses
    .Where(p => p.UserId == userId)
    .AsNoTracking()
    .ToListAsync()
```

`UpsertAsync`:
```csharp
var existing = await context.UserLevelProgresses
    .FirstOrDefaultAsync(p => p.UserId == progress.UserId
                            && p.LevelId == progress.LevelId);
if (existing == null)
    context.UserLevelProgresses.Add(progress);
else
{
    existing.Sbloccato         = progress.Sbloccato;
    existing.Completato        = progress.Completato;
    existing.DataSblocco       = progress.DataSblocco ?? existing.DataSblocco;
    existing.DataCompletamento = progress.DataCompletamento ?? existing.DataCompletamento;
    context.UserLevelProgresses.Update(existing);
}
await context.SaveChangesAsync();
return existing ?? progress;
```

---

## SECTION 8 — INFRASTRUCTURE DI EXTENSION

Append to the existing `MusicApp.Infrastructure/DependencyInjection.cs`
`AddInfrastructure` method. Do not replace the method — add these lines:

```csharp
services.AddScoped<IExerciseTypeRepository, ExerciseTypeRepository>();
services.AddScoped<ILevelRepository, LevelRepository>();
services.AddScoped<IAttemptRepository, AttemptRepository>();
services.AddScoped<IBestScoreRepository, BestScoreRepository>();
services.AddScoped<IUserLevelProgressRepository, UserLevelProgressRepository>();
```

---

## SECTION 9 — APPLICATION DI EXTENSION

Append to the existing `MusicApp.Application/DependencyInjection.cs`
`AddApplication` method. Do not replace the method — add these lines:

```csharp
services.AddScoped<GetExerciseTypesHandler>();
services.AddScoped<GetLevelsHandler>();
services.AddScoped<GetLevelDetailHandler>();
services.AddScoped<GetLevelsByExerciseTypeHandler>();
services.AddScoped<GetMyProgressHandler>();
services.AddScoped<SaveAttemptHandler>();
services.AddScoped<GetMyBestScoresHandler>();
services.AddScoped<GetMyAttemptsHandler>();
```

---

## SECTION 10 — WEB LAYER: CONTROLLERS

### 10.1 General conventions (same as Phase 1)

- Inherit `ControllerBase`, carry `[ApiController]` and `[Route("api/[controller]")]`.
- Return `IActionResult`.
- Extract userId: `int userId = int.Parse(User.FindFirstValue("sub")!);`
- Error → HTTP mapping uses `ApiError` record from Phase 1.
- FluentValidation auto-validates via `[ApiController]`.

### 10.2 `ExerciseTypesController`

File: `MusicApp.Web/Controllers/ExerciseTypesController.cs`
Route prefix: `/api/exercise-types`

#### `GET /api/exercise-types` — anonymous, no `[Authorize]`

1. Call `GetExerciseTypesHandler`.
2. Return `200 OK` with `ExerciseTypeDto[]`.

---

### 10.3 `LevelsController`

File: `MusicApp.Web/Controllers/LevelsController.cs`
Route prefix: `/api/levels`

All endpoints require `[Authorize]` (default policy).

#### `GET /api/levels` — `[Authorize]`

1. Extract `userId`.
2. Call `GetLevelsHandler`.
3. Return `200 OK` with `LevelWithProgressDto[]`.

---

#### `GET /api/levels/{id}` — `[Authorize]`

1. Extract `userId`.
2. Call `GetLevelDetailHandler(userId, id)`.
3. On failure: `LEVEL_NOT_FOUND` → 404.
4. Return `200 OK` with `LevelDetailDto`.

---

#### `GET /api/levels/exercise/{exerciseTypeId}` — `[Authorize]`

IMPORTANT: This route must be declared BEFORE `{id}` in the controller to
avoid ASP.NET routing ambiguity. Use `[HttpGet("exercise/{exerciseTypeId}")]`
explicitly.

1. Extract `userId`.
2. Call `GetLevelsByExerciseTypeHandler(userId, exerciseTypeId)`.
3. On failure: `EXERCISE_TYPE_NOT_FOUND` → 404.
4. Return `200 OK` with `LevelWithProgressDto[]`.

---

### 10.4 `AttemptsController`

File: `MusicApp.Web/Controllers/AttemptsController.cs`
Route prefix: `/api/attempts`

#### `POST /api/attempts` — `[Authorize]`

1. Validate `CreateAttemptRequest` (FluentValidation auto-runs).
2. Extract `userId`.
3. Call `SaveAttemptHandler(userId, request)`.
4. On failure:
   - `EXERCISE_TYPE_NOT_FOUND` → 404
   - `LEVEL_NOT_FOUND` → 404
   - `LEVEL_EXERCISE_MISMATCH` → 422 Unprocessable Entity
   - `INVALID_DIFFICOLTA` → 422
   - `INVALID_PUNTEGGIO` → 422
5. On success: return `201 Created` with `AttemptResultDto` body.
   No `Location` header is required.

---

### 10.5 Additions to `UsersController` (Phase 1 file — extend only)

Add three new action methods to the existing `UsersController`. Do not
modify any existing action.

#### `GET /api/users/me/progress` — `[Authorize]`

1. Extract `userId`.
2. Call `GetMyProgressHandler(userId)`.
3. Return `200 OK` with `UserProgressDto`.

---

#### `GET /api/users/me/best-scores` — `[Authorize]`

1. Extract `userId`.
2. Call `GetMyBestScoresHandler(userId)`.
3. Return `200 OK` with `BestScoreDto[]`.

---

#### `GET /api/users/me/attempts` — `[Authorize]`

Query string: `?exerciseTypeId={id}` (optional).

1. Extract `userId`.
2. Bind query string to `AttemptQuery`.
3. Call `GetMyAttemptsHandler(userId, query)`.
4. Return `200 OK` with `AttemptDto[]`.

---

## SECTION 11 — NO NEW AUTHORIZATION POLICIES

This phase introduces no new authorization policies. All new endpoints use
either no `[Authorize]` attribute (anonymous) or `[Authorize]` with no
policy name (which uses `DefaultPolicy` from Phase 1 — valid JWT +
is_active = "True").

Do NOT register any new policy in `Program.cs` for this phase.

---

## SECTION 12 — VALIDATION CHECKLIST

Before declaring Phase 2 complete, verify every item:

- [ ] `dotnet build` passes with 0 errors across all 4 projects
- [ ] `GET /api/exercise-types` returns 3 items without authentication
- [ ] `GET /api/levels` without JWT returns 401
- [ ] `GET /api/levels` for a new user returns all levels with Sbloccato=true
      only for NumeroLivello=1 entries (ids 1 and 4)
- [ ] `GET /api/levels/exercise/1` returns only levels for Lettura Note
- [ ] `GET /api/levels/exercise/999` returns 404
- [ ] `GET /api/levels/exercise/{id}` does NOT conflict with `GET /api/levels/{id}`
      (routing order is correct)
- [ ] `POST /api/attempts` with ExerciseTypeId=1, LevelId=1, Punteggio=85,
      InputSource="mouse" returns 201 with AttemptId populated
- [ ] `POST /api/attempts` with ExerciseTypeId=3, LevelId=1 (not null) returns
      422 (LevelId must be null for Ritmo)
- [ ] `POST /api/attempts` with ExerciseTypeId=1, LevelId=null returns 422
      (LevelId required for Lettura Note)
- [ ] `POST /api/attempts` with ExerciseTypeId=1, LevelId=4 (belongs to
      Lettura Accordi) returns 422 (LevelExerciseMismatch)
- [ ] `POST /api/attempts` with Punteggio=101 returns 400 (FluentValidation)
- [ ] `POST /api/attempts` with InputSource="joystick" returns 400
- [ ] After saving an attempt with Punteggio >= PunteggioMinimoSblocco for
      Level 1, the response contains LivelloSbloccato populated (Level 2)
      and Level 2 is sbloccato in subsequent GET /api/levels
- [ ] After saving an attempt with Punteggio < PunteggioMinimoSblocco,
      LivelloSbloccato is null in the response
- [ ] First attempt for a user sets IsNewRecord=true
- [ ] Subsequent attempt with lower score sets IsNewRecord=false
- [ ] Subsequent attempt with higher score sets IsNewRecord=true
- [ ] After 51 attempts for the same exercise type, only 50 rows remain
      in the attempts table for that user+type
- [ ] The attempt referenced by best_scores is never deleted during rotation
- [ ] `GET /api/users/me/best-scores` returns one entry per exercise type
      that has at least one attempt
- [ ] `GET /api/users/me/attempts` returns attempts in descending CreatedAt order
- [ ] `GET /api/users/me/attempts?exerciseTypeId=1` filters correctly
- [ ] `GET /api/users/me/progress` returns all exercise types; Livelli is
      empty for Ritmo (ExerciseTypeId=3)

---

## SECTION 13 — OUT OF SCOPE FOR THIS PHASE

Do NOT implement any of the following:

- Frontend exercise rendering (VexFlow, tonal.js, Tone.js, JZZ.js) — these
  are browser-side libraries used in Razor Pages, not backend API concerns
- Score calculation logic — scoring is computed client-side and sent in
  `CreateAttemptRequest.Punteggio`; the server validates range and stores
- Forum posts, comments, votes (Phase 3)
- Teacher profiles, slots, bookings, ratings (Phase 4)
- Stripe payments (Phase 5)
- Chat or notifications (Phases 6–7)
- Admin endpoints (Phase 8)
- ts-fsrs algorithm integration — analytics queries over attempt_errors for
  spaced repetition are a Phase 9 optimization; the data is stored correctly
  in this phase, but no FSRS computation runs server-side yet
- Any Razor Page PageModel beyond stubs already present

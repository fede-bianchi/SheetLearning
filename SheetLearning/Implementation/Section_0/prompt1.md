# SYSTEM PROMPT — Phase 0a + 0b: Infrastructure & Database Setup
# Music Learning App

You are a senior .NET backend engineer. Execute every step in this prompt
sequentially and completely. Do not skip steps. Do not generate application
business logic. Do not generate controllers, services, or use-case handlers.
Scope is strictly: solution scaffolding, Docker environment, EF Core entity
models, DbContext, Fluent API configurations, migrations, and seed data.

---

## SECTION 1 — GLOBAL CONTEXT

**Target framework:** ASP.NET 10
**Architecture pattern:** Clean Architecture — 4-project solution
**Database engine:** MariaDB 11.x
**ORM:** Entity Framework Core 9 with Pomelo.EntityFrameworkCore.MySql provider
**Authentication strategy:** JWT short-lived (15 min access token) +
  Refresh Token in HttpOnly Secure SameSite=Strict cookie. Token revocation
  via `sessions` table. Refresh rotation: every refresh invalidates the old
  token and issues a new one.
**Password hashing:** Argon2id via `Isopoh.Cryptography.Argon2`
**Payments:** Stripe test mode — NOT implemented in this phase
**Frontend:** Razor Pages (server-rendered) + REST API endpoints on the same
  host. No separate frontend project.

---

## SECTION 2 — PHASE 0a: SOLUTION STRUCTURE

### 2.1 Solution layout

Create a solution named `MusicApp` with the following projects:

```
MusicApp.sln
├── src/
│   ├── MusicApp.Domain/          (Class Library — net10.0)
│   ├── MusicApp.Application/     (Class Library — net10.0)
│   ├── MusicApp.Infrastructure/  (Class Library — net10.0)
│   └── MusicApp.Web/             (ASP.NET Core Web App — net10.0)
└── docker-compose.yml
```

Project references:
- `MusicApp.Web` → `MusicApp.Application` + `MusicApp.Infrastructure`
- `MusicApp.Application` → `MusicApp.Domain`
- `MusicApp.Infrastructure` → `MusicApp.Application`

### 2.2 NuGet packages

**MusicApp.Domain** — no external NuGet dependencies.

**MusicApp.Application:**
```
Microsoft.Extensions.DependencyInjection.Abstractions
```

**MusicApp.Infrastructure:**
```
Microsoft.EntityFrameworkCore                    (9.x)
Pomelo.EntityFrameworkCore.MySql                 (9.x)
Microsoft.EntityFrameworkCore.Design             (9.x)
Isopoh.Cryptography.Argon2
Microsoft.AspNetCore.Authentication.JwtBearer
System.IdentityModel.Tokens.Jwt
```

**MusicApp.Web:**
```
Microsoft.EntityFrameworkCore.Design             (9.x)
```

### 2.3 Docker Compose

Create `docker-compose.yml` at solution root:

```yaml
version: "3.9"
services:
  mariadb:
    image: mariadb:11
    container_name: musicapp_db
    restart: unless-stopped
    environment:
      MARIADB_ROOT_PASSWORD: ${DB_ROOT_PASSWORD}
      MARIADB_DATABASE: musicapp
      MARIADB_USER: ${DB_USER}
      MARIADB_PASSWORD: ${DB_PASSWORD}
    ports:
      - "3306:3306"
    volumes:
      - mariadb_data:/var/lib/mysql

  adminer:
    image: adminer:latest
    container_name: musicapp_adminer
    restart: unless-stopped
    ports:
      - "8080:8080"
    depends_on:
      - mariadb

volumes:
  mariadb_data:
```

Create `.env.example` at solution root (committed to repository):

```
DB_ROOT_PASSWORD=changeme
DB_USER=musicapp
DB_PASSWORD=changeme
JWT_SECRET=replace_with_32_char_minimum_secret
JWT_ISSUER=MusicApp
JWT_AUDIENCE=MusicAppUsers
STRIPE_SECRET_KEY=sk_test_placeholder
STRIPE_WEBHOOK_SECRET=whsec_placeholder
```

Create `.env` at solution root (NOT committed — add to `.gitignore`). The
`.env` file is a copy of `.env.example` with real local values.

### 2.4 .gitignore

Generate a standard `.gitignore` for .NET solutions. Additionally ensure these
entries are present:

```
.env
*.user
**/appsettings.Development.json
**/bin/
**/obj/
```

### 2.5 appsettings

**`appsettings.json`** (committed, no secrets):

```json
{
  "ConnectionStrings": {
    "Default": ""
  },
  "Jwt": {
    "Issuer": "",
    "Audience": "",
    "Secret": "",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 30
  },
  "Stripe": {
    "SecretKey": "",
    "WebhookSecret": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

**`appsettings.Development.json`** (NOT committed, listed in `.gitignore`).
This file is where the developer populates all secrets from `.env` for local
runs. Generate it as an empty override shell:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Port=3306;Database=musicapp;User=musicapp;Password=changeme;"
  },
  "Jwt": {
    "Secret": "replace_with_32_char_minimum_secret",
    "Issuer": "MusicApp",
    "Audience": "MusicAppUsers"
  }
}
```

### 2.6 Program.cs skeleton

Generate `Program.cs` in `MusicApp.Web` as a minimal skeleton that:
- Registers `AppDbContext` with the Pomelo MariaDB provider, reading the
  connection string from configuration key `ConnectionStrings:Default`.
  Use `ServerVersion.AutoDetect` or pin to MariaDB 11.
- Registers `Authentication` with `JwtBearer` scheme, reading `Jwt:Issuer`,
  `Jwt:Audience`, `Jwt:Secret` from configuration.
- Registers `Authorization`.
- Maps Razor Pages.
- Maps controllers (empty for now).
- Does NOT register any application services, repositories, or use-cases.
  Leave `// TODO: register application services` comment in place.

---

## SECTION 3 — PHASE 0b: DATABASE SCHEMA

### 3.1 General rules for all entity classes

- All entity classes live in `MusicApp.Domain/Entities/`.
- One file per entity class.
- All primary keys are `int Id` (auto-increment, configured via Fluent API).
- `created_at` columns map to `DateTime CreatedAt` with default value
  `DateTime.UtcNow` set via `HasDefaultValueSql("CURRENT_TIMESTAMP")`.
- `updated_at` columns map to `DateTime UpdatedAt` with
  `ValueGeneratedOnAddOrUpdate()` and
  `HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")`.
- All string columns that have no explicit max length use `VARCHAR(255)` unless
  specified otherwise below.
- All `TEXT` columns map to `string` with `.HasColumnType("TEXT")`.
- All `DECIMAL` columns use `.HasColumnType("DECIMAL(10,2)")`.
- All `TINYINT` columns (non-boolean) map to `byte` with
  `.HasColumnType("TINYINT UNSIGNED")`.
- All `BOOLEAN` columns map to `bool` with `.HasColumnType("TINYINT(1)")`.
- MariaDB does not support `enum` as a CLR type in Pomelo by default.
  Represent all ENUM columns as `string` in the entity and configure them
  with `.HasColumnType("ENUM(...)")` in Fluent API, listing all valid values
  explicitly.
- All Fluent API configuration classes live in
  `MusicApp.Infrastructure/Persistence/Configurations/` as
  `IEntityTypeConfiguration<T>` implementations, one file per entity.
- `AppDbContext` lives in `MusicApp.Infrastructure/Persistence/AppDbContext.cs`
  and calls `modelBuilder.ApplyConfigurationsFromAssembly(...)` to auto-
  discover all configurations.

### 3.2 ENUM value reference

Use these exact string values in `HasColumnType("ENUM(...)")` declarations:

| Column | Valid values |
|---|---|
| `attempts.input_source` | `'mouse','keyboard','midi'` |
| `attempt_errors.element_type` | `'nota','accordo','ritmo'` |
| `votes.target_type` | `'post','comment'` |
| `votes.voto` | `'upvote','downvote'` |
| `lesson_bookings.stato` | `'proposta','confermata','cancellata','completata'` |
| `payments.stato` | `'pending','completed','failed','refunded'` |
| `payments.tipo` | `'abbonamento_pro','bundle_lezioni','lezione_singola'` |
| `teacher_profiles.visibile_a` | `'tutti','solo_pro','nessuno'` |
| `teacher_categories.categoria` | `'principiante','intermedio','avanzato'` |

### 3.3 Entity definitions

Generate one entity class and one `IEntityTypeConfiguration<T>` per table
listed below. Apply all constraints, indexes, and FK cascade rules as specified.
Use `DeleteBehavior.Restrict` on all FKs unless noted otherwise.

---

#### Table: `roles`

Entity: `Role`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| nome | Nome | string | NOT NULL, VARCHAR(50) |
| descrizione | Descrizione | string | TEXT, nullable |

---

#### Table: `plans`

Entity: `Plan`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| nome | Nome | string | NOT NULL, VARCHAR(50) |
| descrizione | Descrizione | string | TEXT, nullable |
| prezzo_mensile | PrezzoMensile | decimal | NOT NULL, DECIMAL(10,2) |

---

#### Table: `users`

Entity: `User`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| nome | Nome | string | NOT NULL |
| cognome | Cognome | string | NOT NULL |
| nickname | Nickname | string | NOT NULL, UNIQUE |
| email | Email | string | NOT NULL, UNIQUE |
| password_hash | PasswordHash | string | NOT NULL |
| data_nascita | DataNascita | DateOnly | NOT NULL |
| descrizione | Descrizione | string | TEXT, nullable |
| strumento | Strumento | string | nullable, default `'Nessuno'` via `HasDefaultValue("Nessuno")` |
| role_id | RoleId | int | NOT NULL, FK → roles.id |
| plan_id | PlanId | int | NOT NULL, FK → plans.id |
| is_active | IsActive | bool | NOT NULL, default true |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| updated_at | UpdatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP ON UPDATE |

Navigation properties: `Role Role`, `Plan Plan`.

---

#### Table: `sessions`

Entity: `Session`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Cascade |
| token | Token | string | NOT NULL, UNIQUE |
| expires_at | ExpiresAt | DateTime | NOT NULL |
| ip_address | IpAddress | string | nullable |
| user_agent | UserAgent | string | TEXT, nullable |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Navigation: `User User`.

---

#### Table: `exercise_types`

Entity: `ExerciseType`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| nome | Nome | string | NOT NULL, UNIQUE, VARCHAR(50) |
| descrizione | Descrizione | string | TEXT, nullable |

---

#### Table: `clefs`

Entity: `Clef`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| nome | Nome | string | NOT NULL, UNIQUE, VARCHAR(50) |

---

#### Table: `levels`

Entity: `Level`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| exercise_type_id | ExerciseTypeId | int | NOT NULL, FK → exercise_types.id |
| clef_id | ClefId | int? | nullable, FK → clefs.id — Ritmo has no clef |
| numero_livello | NumeroLivello | byte | NOT NULL, TINYINT UNSIGNED, values 1/2/3 |
| nome | Nome | string | NOT NULL |
| descrizione | Descrizione | string | TEXT, nullable |
| punteggio_minimo_sblocco | PunteggioMinimoSblocco | int | NOT NULL |

Navigation: `ExerciseType ExerciseType`, `Clef? Clef`.

---

#### Table: `user_level_progress`

Entity: `UserLevelProgress`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Cascade |
| level_id | LevelId | int | NOT NULL, FK → levels.id |
| sbloccato | Sbloccato | bool | NOT NULL, default false |
| completato | Completato | bool | NOT NULL, default false |
| data_sblocco | DataSblocco | DateTime? | nullable |
| data_completamento | DataCompletamento | DateTime? | nullable |

Navigation: `User User`, `Level Level`.

---

#### Table: `attempts`

Entity: `Attempt`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Cascade |
| exercise_type_id | ExerciseTypeId | int | NOT NULL, FK → exercise_types.id |
| level_id | LevelId | int? | nullable, FK → levels.id — null for Ritmo |
| difficolta | Difficolta | byte? | nullable, TINYINT UNSIGNED, range 0-30, only for Ritmo |
| punteggio | Punteggio | int | NOT NULL, range 0-100 |
| tempo_risposta_ms | TempoRispostaMs | int? | nullable — latency in ms for note reading |
| exercise_subtype | ExerciseSubtype | string? | nullable, VARCHAR(30) — 'riconoscimento'/'esecuzione', only for Ritmo |
| tolerance_window_ms | ToleranceWindowMs | int? | nullable — rhythm tolerance used (150/100/50 ms) |
| input_source | InputSource | string | NOT NULL, ENUM('mouse','keyboard','midi') |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Navigation: `User User`, `ExerciseType ExerciseType`, `Level? Level`,
`ICollection<AttemptError> AttemptErrors`.

---

#### Table: `attempt_errors`

Entity: `AttemptError`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| attempt_id | AttemptId | int | NOT NULL, FK → attempts.id, DeleteBehavior.Cascade |
| element_type | ElementType | string | NOT NULL, ENUM('nota','accordo','ritmo') |
| risposta_data | RispostaData | string | NOT NULL, VARCHAR(100) |
| risposta_corretta | RispostaCorretta | string | NOT NULL, VARCHAR(100) |
| posizione_nel_pattern | PosizioneNelPattern | byte? | nullable, TINYINT UNSIGNED — beat position, only for Ritmo |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Indexes (create explicitly in Fluent API):
- `HasIndex(e => e.AttemptId)` — fast join
- `HasIndex(e => new { e.AttemptId, e.ElementType })` — FSRS analytics

Navigation: `Attempt Attempt`.

---

#### Table: `best_scores`

Entity: `BestScore`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Cascade |
| exercise_type_id | ExerciseTypeId | int | NOT NULL, FK → exercise_types.id |
| punteggio_migliore | PunteggioMigliore | int | NOT NULL |
| attempt_id | AttemptId | int | NOT NULL, FK → attempts.id — the record attempt |
| updated_at | UpdatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP ON UPDATE |

Unique constraint: `HasIndex(e => new { e.UserId, e.ExerciseTypeId }).IsUnique()`

Navigation: `User User`, `ExerciseType ExerciseType`, `Attempt Attempt`.

---

#### Table: `posts`

Entity: `Post`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| titolo | Titolo | string | NOT NULL |
| contenuto | Contenuto | string | NOT NULL, TEXT |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| updated_at | UpdatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP ON UPDATE |
| deleted_at | DeletedAt | DateTime? | nullable — soft delete timestamp |
| is_deleted | IsDeleted | bool | NOT NULL, default false |

Navigation: `User User`, `ICollection<Comment> Comments`.

---

#### Table: `comments`

Entity: `Comment`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| post_id | PostId | int | NOT NULL, FK → posts.id, DeleteBehavior.Cascade |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| parent_comment_id | ParentCommentId | int? | nullable, FK → comments.id (self-ref), DeleteBehavior.Restrict — NULL = direct reply to post |
| contenuto | Contenuto | string | NOT NULL, TEXT |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| updated_at | UpdatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP ON UPDATE |
| deleted_at | DeletedAt | DateTime? | nullable |
| is_deleted | IsDeleted | bool | NOT NULL, default false |

Navigation: `User User`, `Post Post`, `Comment? ParentComment`,
`ICollection<Comment> Replies`.

---

#### Table: `votes`

Entity: `Vote`

Polymorphic vote table covering both posts and comments.

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Cascade |
| target_type | TargetType | string | NOT NULL, ENUM('post','comment') |
| target_id | TargetId | int | NOT NULL — ID of the post or comment voted on (no FK, polymorphic) |
| voto | Voto | string | NOT NULL, ENUM('upvote','downvote') |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Unique constraint:
`HasIndex(e => new { e.UserId, e.TargetType, e.TargetId }).IsUnique()`

Note: `target_id` has NO database-level foreign key constraint because it is
polymorphic. Referential integrity is enforced at the application layer.

Navigation: `User User`.

---

#### Table: `lesson_bundles`

Entity: `LessonBundle`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| nome | Nome | string | NOT NULL |
| numero_lezioni | NumeroLezioni | int | NOT NULL |
| prezzo | Prezzo | decimal | NOT NULL, DECIMAL(10,2) |
| sconto_percentuale | ScontoPercentuale | decimal | NOT NULL, DECIMAL(5,2), default 0 |
| is_active | IsActive | bool | NOT NULL, default true |
| expires_after_days | ExpiresAfterDays | int? | nullable — null means no expiry |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

---

#### Table: `lesson_slots`

Entity: `LessonSlot`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| teacher_id | TeacherId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| data_ora_inizio | DataOraInizio | DateTime | NOT NULL |
| data_ora_fine | DataOraFine | DateTime | NOT NULL |
| is_available | IsAvailable | bool | NOT NULL, default true |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Navigation: `User Teacher`.

---

#### Table: `payments`

Entity: `Payment`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| importo | Importo | decimal | NOT NULL, DECIMAL(10,2) |
| valuta | Valuta | string | NOT NULL, VARCHAR(3), default 'EUR' |
| stato | Stato | string | NOT NULL, ENUM('pending','completed','failed','refunded') |
| metodo_pagamento | MetodoPagamento | string | nullable, VARCHAR(50) |
| riferimento_esterno | RiferimentoEsterno | string | nullable — Stripe transaction ID |
| tipo | Tipo | string | NOT NULL, ENUM('abbonamento_pro','bundle_lezioni','lezione_singola') |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| updated_at | UpdatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP ON UPDATE |

Navigation: `User User`.

---

#### Table: `lesson_bundle_purchases`

Entity: `LessonBundlePurchase`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| bundle_id | BundleId | int | NOT NULL, FK → lesson_bundles.id, DeleteBehavior.Restrict |
| lezioni_totali | LezioniTotali | int | NOT NULL — copied from bundle at purchase time |
| lezioni_usate | LezioniUsate | int | NOT NULL, default 0 |
| payment_id | PaymentId | int | NOT NULL, FK → payments.id, DeleteBehavior.Restrict |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| expires_at | ExpiresAt | DateTime? | nullable |

Navigation: `User User`, `LessonBundle Bundle`, `Payment Payment`.

---

#### Table: `lesson_bookings`

Entity: `LessonBooking`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| student_id | StudentId | int | NOT NULL, FK → users.id (student), DeleteBehavior.Restrict |
| teacher_id | TeacherId | int | NOT NULL, FK → users.id (teacher), DeleteBehavior.Restrict |
| slot_id | SlotId | int | NOT NULL, FK → lesson_slots.id, DeleteBehavior.Restrict |
| bundle_purchase_id | BundlePurchaseId | int? | nullable, FK → lesson_bundle_purchases.id |
| stato | Stato | string | NOT NULL, ENUM('proposta','confermata','cancellata','completata') |
| note_studente | NoteStudente | string | TEXT, nullable |
| note_insegnante | NoteInsegnante | string | TEXT, nullable |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| updated_at | UpdatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP ON UPDATE |

Note: `student_id` and `teacher_id` both reference `users.id`. Configure
two separate `HasOne` / `WithMany` relationships using explicit FK column names
to avoid EF Core ambiguity.

Navigation: `User Student`, `User Teacher`, `LessonSlot Slot`,
`LessonBundlePurchase? BundlePurchase`, `LessonRating? Rating`.

---

#### Table: `subscriptions`

Entity: `Subscription`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| plan_id | PlanId | int | NOT NULL, FK → plans.id |
| payment_id | PaymentId | int | NOT NULL, FK → payments.id |
| data_inizio | DataInizio | DateOnly | NOT NULL |
| data_fine | DataFine | DateOnly | NOT NULL |
| is_active | IsActive | bool | NOT NULL, default true |
| rinnovo_automatico | RinnovoAutomatico | bool | NOT NULL, default true |

Navigation: `User User`, `Plan Plan`, `Payment Payment`.

---

#### Table: `chats`

Entity: `Chat`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| student_id | StudentId | int | NOT NULL, FK → users.id (student), DeleteBehavior.Restrict |
| teacher_id | TeacherId | int | NOT NULL, FK → users.id (teacher), DeleteBehavior.Restrict |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| last_message_at | LastMessageAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Unique constraint: `HasIndex(e => new { e.StudentId, e.TeacherId }).IsUnique()`

Note: `student_id` and `teacher_id` both reference `users.id`. Configure
two separate named relationships as with `lesson_bookings`.

Navigation: `User Student`, `User Teacher`,
`ICollection<ChatMessage> Messages`.

---

#### Table: `chat_messages`

Entity: `ChatMessage`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| chat_id | ChatId | int | NOT NULL, FK → chats.id, DeleteBehavior.Cascade |
| sender_id | SenderId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| contenuto | Contenuto | string | NOT NULL, TEXT |
| letto | Letto | bool | NOT NULL, default false |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Navigation: `Chat Chat`, `User Sender`.

---

#### Table: `notifications`

Entity: `Notification`

Valid `tipo` values (stored as VARCHAR, not ENUM — extensible by design):
`lezione_proposta`, `lezione_confermata`, `lezione_cancellata`,
`lezione_completata`, `messaggio_ricevuto`, `commento_risposta`,
`post_risposta`, `livello_sbloccato`, `record_battuto`,
`bundle_in_scadenza`, `abbonamento_in_scadenza`, `moderazione_ricevuta`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, DeleteBehavior.Cascade |
| tipo | Tipo | string | NOT NULL, VARCHAR(50) |
| titolo | Titolo | string | NOT NULL |
| corpo | Corpo | string | TEXT, nullable |
| target_type | TargetType | string | nullable, VARCHAR(50) |
| target_id | TargetId | int? | nullable |
| is_read | IsRead | bool | NOT NULL, default false |
| is_archived | IsArchived | bool | NOT NULL, default false |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| read_at | ReadAt | DateTime? | nullable |

Navigation: `User User`.

---

#### Table: `moderation_logs`

Entity: `ModerationLog`

Append-only audit log. No `updated_at` column. Do not call
`ValueGeneratedOnAddOrUpdate` on any column in this entity.

Valid `azione` values (VARCHAR, not ENUM):
`delete_post`, `delete_comment`, `ban_user`, `unban_user`, `warn_user`

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| admin_id | AdminId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| target_user_id | TargetUserId | int? | nullable, FK → users.id — the user targeted by the action |
| azione | Azione | string | NOT NULL, VARCHAR(50) |
| target_type | TargetType | string | nullable, VARCHAR(50) — 'post'/'comment'/'user' |
| target_id | TargetId | int? | nullable — ID of removed content |
| motivazione | Motivazione | string | NOT NULL, TEXT |
| contenuto_rimosso | ContenutoRimosso | string | TEXT, nullable — copy of removed text for audit trail |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Note: `admin_id` and `target_user_id` both reference `users.id`. Configure
two separate named relationships.

Navigation: `User Admin`, `User? TargetUser`.

---

#### Table: `teacher_profiles`

Entity: `TeacherProfile`

One-to-one with `users` (only users with role Insegnante have a profile).

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| user_id | UserId | int | NOT NULL, FK → users.id, UNIQUE, DeleteBehavior.Cascade |
| bio | Bio | string | TEXT, nullable |
| specializzazioni | Specializzazioni | string | TEXT, nullable |
| visibile_a | VisibileA | string | NOT NULL, ENUM('tutti','solo_pro','nessuno'), default 'tutti' |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |
| updated_at | UpdatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP ON UPDATE |

Navigation: `User User`, `ICollection<TeacherCategory> Categories`.

---

#### Table: `teacher_categories`

Entity: `TeacherCategory`

Self-declared categories visible to all users for filtering.
Max 3 per teacher is enforced at application layer only (no DB-level check).

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| teacher_id | TeacherId | int | NOT NULL, FK → teacher_profiles.id, DeleteBehavior.Cascade |
| categoria | Categoria | string | NOT NULL, ENUM('principiante','intermedio','avanzato') |

Unique constraint:
`HasIndex(e => new { e.TeacherId, e.Categoria }).IsUnique()`

Navigation: `TeacherProfile TeacherProfile`.

---

#### Table: `lesson_ratings`

Entity: `LessonRating`

One rating per booking. Visible only to Admin.
Rating window: 48h from `lesson_bookings.updated_at` when `stato = 'completata'`.

| Column | C# property | Type | Constraints |
|---|---|---|---|
| id | Id | int | PK, auto-increment |
| booking_id | BookingId | int | NOT NULL, FK → lesson_bookings.id, UNIQUE, DeleteBehavior.Restrict |
| student_id | StudentId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| teacher_id | TeacherId | int | NOT NULL, FK → users.id, DeleteBehavior.Restrict |
| valutazione | Valutazione | byte | NOT NULL, TINYINT UNSIGNED, values 1/2/3 |
| created_at | CreatedAt | DateTime | NOT NULL, default CURRENT_TIMESTAMP |

Unique constraint on `booking_id`:
`HasIndex(e => e.BookingId).IsUnique()`

Note: `student_id` and `teacher_id` both reference `users.id`. Configure
two separate named relationships.

Navigation: `LessonBooking Booking`, `User Student`, `User Teacher`.

---

### 3.4 Migration

After generating all entity classes and configurations, run:

```bash
dotnet ef migrations add InitialCreate \
  --project src/MusicApp.Infrastructure \
  --startup-project src/MusicApp.Web \
  --output-dir Persistence/Migrations
```

Then apply:

```bash
dotnet ef database update \
  --project src/MusicApp.Infrastructure \
  --startup-project src/MusicApp.Web
```

The migration must succeed with zero errors. If EF Core raises ambiguous
navigation warnings due to multi-FK relationships on `users`, resolve them
by adding explicit `.HasForeignKey()` and `.HasPrincipalKey()` calls in the
affected configuration classes (`LessonBooking`, `Chat`, `ModerationLog`,
`LessonRating`).

---

### 3.5 Seed data

After the migration applies successfully, create a class
`MusicApp.Infrastructure/Persistence/DbSeeder.cs` with a static method
`SeedAsync(AppDbContext context)`. This method must be idempotent (check
existence before inserting). Seed the following lookup data:

**roles:**
```
{ Id=1, Nome="Admin",       Descrizione="Amministratore piattaforma" }
{ Id=2, Nome="Insegnante",  Descrizione="Insegnante di musica" }
{ Id=3, Nome="Utente Pro",  Descrizione="Utente con piano Pro attivo" }
{ Id=4, Nome="Utente",      Descrizione="Utente standard" }
```

**plans:**
```
{ Id=1, Nome="Standard", Descrizione="Piano gratuito",   PrezzoMensile=0.00 }
{ Id=2, Nome="Pro",      Descrizione="Piano Pro mensile", PrezzoMensile=9.99 }
```

**exercise_types:**
```
{ Id=1, Nome="Lettura Note",    Descrizione="Riconoscimento visivo di note sul pentagramma" }
{ Id=2, Nome="Lettura Accordi", Descrizione="Riconoscimento visivo di accordi sul pentagramma" }
{ Id=3, Nome="Ritmo",           Descrizione="Esercizi di lettura ed esecuzione ritmica" }
```

**clefs:**
```
{ Id=1, Nome="Chiave di Sol" }
{ Id=2, Nome="Chiave di Basso" }
```

**levels** — seed only Lettura Note and Lettura Accordi.
Ritmo uses `difficolta 0-30` and does NOT get rows in `levels`:

```
Lettura Note (exercise_type_id=1):
{ Id=1, ExerciseTypeId=1, ClefId=1, NumeroLivello=1,
  Nome="Note centrali",
  Descrizione="Do4-Sol4, chiave di Sol",
  PunteggioMinimoSblocco=70 }

{ Id=2, ExerciseTypeId=1, ClefId=1, NumeroLivello=2,
  Nome="Rigo completo",
  Descrizione="Estensione righe supplementari, chiave di Sol completa",
  PunteggioMinimoSblocco=75 }

{ Id=3, ExerciseTypeId=1, ClefId=2, NumeroLivello=3,
  Nome="Chiave di Basso e lettura mista",
  Descrizione="Chiave di Basso, poi lettura combinata Sol e Basso",
  PunteggioMinimoSblocco=80 }

Lettura Accordi (exercise_type_id=2):
{ Id=4, ExerciseTypeId=2, ClefId=1, NumeroLivello=1,
  Nome="Triadi fondamentali",
  Descrizione="Triadi maggiori e minori in posizione fondamentale",
  PunteggioMinimoSblocco=70 }

{ Id=5, ExerciseTypeId=2, ClefId=1, NumeroLivello=2,
  Nome="Accordi di settima",
  Descrizione="Maj7, Dom7, Min7",
  PunteggioMinimoSblocco=75 }

{ Id=6, ExerciseTypeId=2, ClefId=1, NumeroLivello=3,
  Nome="Inversioni",
  Descrizione="Inversioni di triadi e accordi di settima",
  PunteggioMinimoSblocco=80 }
```

IMPORTANT: `PunteggioMinimoSblocco` values above are provisional placeholders.
They will be revised in a later phase based on real usage data. Do NOT hardcode
them as constants anywhere in application logic — always read them from the
database at runtime.

Call `DbSeeder.SeedAsync(context)` inside `Program.cs` after
`app.UseAuthentication()` / `app.UseAuthorization()` and before `app.Run()`,
wrapped in a scoped service resolution block.

---

## SECTION 4 — VALIDATION CHECKLIST

Before declaring the phase complete, verify every item below is true:

- [ ] `dotnet build` passes with 0 errors and 0 warnings across all 4 projects
- [ ] `dotnet ef migrations add InitialCreate` generates a non-empty migration
      file containing all 22 tables
- [ ] `dotnet ef database update` applies cleanly against a running MariaDB 11
      container started via `docker-compose up -d mariadb`
- [ ] `DbSeeder.SeedAsync` completes without exception on a freshly migrated DB
- [ ] Running `DbSeeder.SeedAsync` twice does not create duplicate rows
      (idempotency guaranteed)
- [ ] No entity class in `MusicApp.Domain` imports any namespace from
      `MusicApp.Web` or `MusicApp.Infrastructure`
- [ ] All multi-FK relationships on `users` are explicitly disambiguated in
      Fluent API — zero shadow FK warnings in EF Core output

---

## SECTION 5 — OUT OF SCOPE FOR THIS PHASE

Do NOT generate any of the following. They belong to later phases:

- JWT token generation or validation logic
- Password hashing or verification logic
- Any controller, API endpoint, Razor Page, or PageModel
- Any repository interface or implementation
- Any application service or use-case handler
- Any DTO, mapping profile, or AutoMapper configuration
- Stripe integration code of any kind
- SignalR hubs or real-time infrastructure
- Any middleware beyond the minimal Program.cs skeleton

If you find yourself writing business logic, stop and return to infrastructure
scope.

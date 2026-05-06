# SYSTEM PROMPT — Phase 1: Authentication, Sessions & User Profile
# Music Learning App

You are a senior .NET backend engineer continuing work on a solution scaffolded
in Phase 0. The database schema, EF Core entities, DbContext, Fluent API
configurations, migrations, and seed data are already complete and applied.
Do not modify any entity class, configuration, or migration from Phase 0.

Your scope in this phase:
- Application layer: interfaces, use-case command/query objects, result types
- Infrastructure layer: repository implementations, JWT service, Argon2id
  password hasher, refresh token cookie helper
- Web layer: API controllers, DTOs, FluentValidation validators, ASP.NET Core
  authorization policies strictly required by this phase's endpoints
- Program.cs: register all new services, policies, and middleware added here

---

## SECTION 1 — PREREQUISITES & CONSTRAINTS

**Phase 0 outputs (already exist, do not touch):**
- `MusicApp.Domain/Entities/` — all 22 entity classes
- `MusicApp.Infrastructure/Persistence/AppDbContext.cs`
- `MusicApp.Infrastructure/Persistence/Configurations/` — all Fluent API configs
- `MusicApp.Infrastructure/Persistence/Migrations/` — InitialCreate migration
- `MusicApp.Infrastructure/Persistence/DbSeeder.cs`
- `MusicApp.Web/Program.cs` — skeleton with DbContext, Authentication,
  Authorization, Razor Pages, and Controllers registered

**Architecture rules:**
- `MusicApp.Domain` — entities only. No interfaces, no services. Already complete.
- `MusicApp.Application` — interfaces (ports), use-case handlers, DTOs,
  result/error types, FluentValidation validators. No EF Core references.
- `MusicApp.Infrastructure` — implements Application interfaces. Has EF Core,
  Pomelo, Argon2, JWT. No direct reference from Domain or Application to
  Infrastructure — dependency flows inward only.
- `MusicApp.Web` — controllers, Razor Pages (stub only this phase),
  ASP.NET policies, DI wiring. Calls Application use-case handlers directly.

**Naming conventions:**
- Use-case handlers follow the pattern: `{Action}{Resource}Handler`
  e.g., `RegisterUserHandler`, `LoginUserHandler`, `RefreshTokenHandler`
- Repository interfaces: `I{Entity}Repository` in `MusicApp.Application/Interfaces/`
- All handlers live in `MusicApp.Application/UseCases/Auth/` or
  `MusicApp.Application/UseCases/Users/`
- All DTOs for this phase live in `MusicApp.Application/DTOs/`
- All validators live in `MusicApp.Application/Validators/`

---

## SECTION 2 — APPLICATION LAYER: INTERFACES

Create the following interfaces in `MusicApp.Application/Interfaces/`.

### 2.1 `IUserRepository`

```csharp
Task<User?> GetByIdAsync(int id);
Task<User?> GetByEmailAsync(string email);
Task<User?> GetByNicknameAsync(string nickname);
Task<bool> EmailExistsAsync(string email);
Task<bool> NicknameExistsAsync(string nickname);
Task<User> CreateAsync(User user);
Task<User> UpdateAsync(User user);
```

### 2.2 `ISessionRepository`

```csharp
Task<Session?> GetByTokenAsync(string token);
Task<Session> CreateAsync(Session session);
Task DeleteAsync(Session session);
Task DeleteAllForUserAsync(int userId);
Task DeleteExpiredAsync(); // called on startup or background — purge expired rows
```

### 2.3 `IPasswordHasher`

```csharp
string Hash(string plaintext);
bool Verify(string plaintext, string hash);
```

### 2.4 `IJwtService`

```csharp
string GenerateAccessToken(User user);
// Returns the raw refresh token string (random, opaque, stored in sessions table)
string GenerateRefreshToken();
```

### 2.5 `IRefreshCookieHelper`

```csharp
void SetRefreshCookie(HttpResponse response, string token, DateTime expiresAt);
void ClearRefreshCookie(HttpResponse response);
string? ReadRefreshToken(HttpRequest request);
```

This interface lives in `MusicApp.Application/Interfaces/` but its
implementation references `Microsoft.AspNetCore.Http`. The implementation
lives in `MusicApp.Web/Infrastructure/` (not in `MusicApp.Infrastructure`
project) to keep HTTP concerns in the Web project.

---

## SECTION 3 — APPLICATION LAYER: RESULT TYPE

Create a generic result type used by all use-case handlers to avoid throwing
exceptions for predictable domain errors.

File: `MusicApp.Application/Common/Result.cs`

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(string code, string message)
    { IsSuccess = false; ErrorCode = code; ErrorMessage = message; }

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(string code, string message) => new(code, message);
}

public static class ErrorCodes
{
    // Auth
    public const string EmailAlreadyExists    = "EMAIL_ALREADY_EXISTS";
    public const string NicknameAlreadyExists = "NICKNAME_ALREADY_EXISTS";
    public const string InvalidCredentials    = "INVALID_CREDENTIALS";
    public const string AccountInactive       = "ACCOUNT_INACTIVE";
    public const string InvalidRefreshToken   = "INVALID_REFRESH_TOKEN";
    public const string ExpiredRefreshToken   = "EXPIRED_REFRESH_TOKEN";
    // Users
    public const string UserNotFound          = "USER_NOT_FOUND";
    public const string WrongPassword         = "WRONG_PASSWORD";
    public const string NicknameConflict      = "NICKNAME_CONFLICT";
    public const string UnderAge              = "UNDER_AGE";
}
```

---

## SECTION 4 — APPLICATION LAYER: DTOs

All DTO classes live in `MusicApp.Application/DTOs/`. These are plain C# record
or class types with no EF Core or ASP.NET attributes.

### 4.1 `UserDto` (shared — returned in AuthResponse and elsewhere)

```csharp
public record UserDto(
    int    Id,
    string Nome,
    string Cognome,
    string Nickname,
    string Email,
    string Ruolo,        // "Admin" / "Insegnante" / "Utente Pro" / "Utente"
    string Piano,        // "Standard" / "Pro"
    bool   IsActive,
    string? Strumento
);
```

### 4.2 `AuthResponse`

```csharp
public record AuthResponse(
    string  AccessToken,
    int     ExpiresIn,   // seconds — always 900 (15 min)
    UserDto User
);
```

### 4.3 `UserProfileDto`

```csharp
public record UserProfileDto(
    int      Id,
    string   Nome,
    string   Cognome,
    string   Nickname,
    string   Email,
    string   Ruolo,
    string   Piano,
    bool     IsActive,
    string?  Strumento,
    string?  Descrizione,
    DateOnly DataNascita,
    DateTime CreatedAt
);
```

### 4.4 `PublicUserDto`

```csharp
public record PublicUserDto(
    int     Id,
    string  Nickname,
    string? Strumento,
    string? Descrizione
);
```

### 4.5 Request DTOs (used as controller input — can have DataAnnotations
    but primary validation is FluentValidation)

```csharp
public record RegisterRequest(
    string   Nome,
    string   Cognome,
    string   Nickname,
    string   Email,
    string   Password,
    DateOnly DataNascita,
    string?  Strumento
);

public record LoginRequest(
    string Email,
    string Password
);

public record UpdateProfileRequest(
    string?  Nome,
    string?  Cognome,
    string?  Nickname,
    string?  Descrizione,
    string?  Strumento
);

public record ChangePasswordRequest(
    string PasswordOld,
    string PasswordNew
);

public record DeleteAccountRequest(
    string ConfermaPassword   // user must re-enter current password to confirm
);
```

---

## SECTION 5 — APPLICATION LAYER: VALIDATORS

Use FluentValidation. All validators live in `MusicApp.Application/Validators/`.
Register them with `services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>()`
in the Application DI extension method.

### 5.1 `RegisterRequestValidator`

| Field | Rules |
|---|---|
| Nome | NotEmpty, MaxLength(100) |
| Cognome | NotEmpty, MaxLength(100) |
| Nickname | NotEmpty, MinLength(3), MaxLength(50), matches `^[a-zA-Z0-9_.-]+$` |
| Email | NotEmpty, valid email format via `.EmailAddress()` |
| Password | NotEmpty, MinLength(8), MaxLength(128) |
| DataNascita | NotEmpty, must be at least 18 years before `DateOnly.FromDateTime(DateTime.UtcNow)` — compute inline as `dataNascita <= DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-18)` |
| Strumento | Optional, MaxLength(100) when provided |

### 5.2 `LoginRequestValidator`

| Field | Rules |
|---|---|
| Email | NotEmpty, valid email format |
| Password | NotEmpty |

### 5.3 `UpdateProfileRequestValidator`

At least one field must be non-null (custom rule: `Must(r => r.Nome != null || r.Cognome != null || r.Nickname != null || r.Descrizione != null || r.Strumento != null)`).
When each field is provided: same length/format rules as RegisterRequestValidator.

### 5.4 `ChangePasswordRequestValidator`

| Field | Rules |
|---|---|
| PasswordOld | NotEmpty |
| PasswordNew | NotEmpty, MinLength(8), MaxLength(128) |
| PasswordNew | Must not equal PasswordOld (prevent no-op change) |

### 5.5 `DeleteAccountRequestValidator`

| Field | Rules |
|---|---|
| ConfermaPassword | NotEmpty |

---

## SECTION 6 — APPLICATION LAYER: USE-CASE HANDLERS

All handlers live in `MusicApp.Application/UseCases/`. Each handler receives
its dependencies via constructor injection (interfaces only — no concrete types).

### 6.1 `RegisterUserHandler`

File: `MusicApp.Application/UseCases/Auth/RegisterUserHandler.cs`

Dependencies: `IUserRepository`, `IPasswordHasher`, `IJwtService`,
`ISessionRepository`, `IOptions<JwtOptions>`

Logic:
1. Check `IUserRepository.EmailExistsAsync` — if true, return
   `Result.Fail(ErrorCodes.EmailAlreadyExists, "...")`.
2. Check `IUserRepository.NicknameExistsAsync` — if true, return
   `Result.Fail(ErrorCodes.NicknameAlreadyExists, "...")`.
3. Age check: if `DateOnly.FromDateTime(DateTime.UtcNow).Year - request.DataNascita.Year`
   (accounting for month/day) < 18, return
   `Result.Fail(ErrorCodes.UnderAge, "...")`.
4. Hash password with `IPasswordHasher.Hash`.
5. Create `User` entity. Set `RoleId = 4` (Utente), `PlanId = 1` (Standard),
   `IsActive = true`. Persist via `IUserRepository.CreateAsync`.
6. Generate access token via `IJwtService.GenerateAccessToken`.
7. Generate refresh token via `IJwtService.GenerateRefreshToken`.
8. Create `Session` entity with `UserId`, `Token = refreshToken`,
   `ExpiresAt = DateTime.UtcNow.AddDays(JwtOptions.RefreshTokenExpiryDays)`.
   Persist via `ISessionRepository.CreateAsync`.
9. Return `Result.Ok(new AuthResponse(...))` along with the raw refresh token
   string and its expiry (the controller will write the cookie).

Return type: `Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>`

### 6.2 `LoginUserHandler`

File: `MusicApp.Application/UseCases/Auth/LoginUserHandler.cs`

Dependencies: `IUserRepository`, `IPasswordHasher`, `IJwtService`,
`ISessionRepository`, `IOptions<JwtOptions>`

Logic:
1. Load user by email. If null, return `Result.Fail(ErrorCodes.InvalidCredentials, "...")`.
   Use a constant-time response — do not distinguish "email not found" from
   "wrong password" in the error message exposed to the client.
2. If `user.IsActive == false`, return `Result.Fail(ErrorCodes.AccountInactive, "...")`.
3. Verify password with `IPasswordHasher.Verify`. If false, return
   `Result.Fail(ErrorCodes.InvalidCredentials, "...")`.
4. Generate access token, generate refresh token.
5. Persist new `Session`. Return `Result.Ok(...)` with same tuple shape as Register.

### 6.3 `RefreshTokenHandler`

File: `MusicApp.Application/UseCases/Auth/RefreshTokenHandler.cs`

Dependencies: `ISessionRepository`, `IUserRepository`, `IJwtService`,
`IOptions<JwtOptions>`

Logic:
1. Look up session by token string via `ISessionRepository.GetByTokenAsync`.
   If null, return `Result.Fail(ErrorCodes.InvalidRefreshToken, "...")`.
2. If `session.ExpiresAt <= DateTime.UtcNow`, delete the session, return
   `Result.Fail(ErrorCodes.ExpiredRefreshToken, "...")`.
3. Load the user. If `user.IsActive == false`, delete the session, return
   `Result.Fail(ErrorCodes.AccountInactive, "...")`.
4. TOKEN ROTATION: delete the old session
   (`ISessionRepository.DeleteAsync(session)`).
5. Generate new access token and new refresh token.
6. Persist new session with new token and new `ExpiresAt`.
7. Return `Result.Ok(...)` with tuple including new tokens.

### 6.4 `LogoutHandler`

File: `MusicApp.Application/UseCases/Auth/LogoutHandler.cs`

Dependencies: `ISessionRepository`

Logic:
1. Accept the raw refresh token string extracted from the cookie by the caller.
2. Look up session by token. If not found, return silently (idempotent — no error).
3. Delete the session.
4. Return `Result.Ok(true)`.

### 6.5 `LogoutAllHandler`

File: `MusicApp.Application/UseCases/Auth/LogoutAllHandler.cs`

Dependencies: `ISessionRepository`

Logic:
1. Accept `int userId` extracted from the JWT claim by the caller.
2. Call `ISessionRepository.DeleteAllForUserAsync(userId)`.
3. Return `Result.Ok(true)`.

### 6.6 `GetMyProfileHandler`

File: `MusicApp.Application/UseCases/Users/GetMyProfileHandler.cs`

Dependencies: `IUserRepository`

Logic:
1. Load user by id from JWT claim.
2. If null, return `Result.Fail(ErrorCodes.UserNotFound, "...")`.
3. Map to `UserProfileDto`. Return `Result.Ok(dto)`.

### 6.7 `UpdateMyProfileHandler`

File: `MusicApp.Application/UseCases/Users/UpdateMyProfileHandler.cs`

Dependencies: `IUserRepository`

Logic:
1. Load user by id.
2. If `request.Nickname` is provided and different from current, check uniqueness.
   If taken, return `Result.Fail(ErrorCodes.NicknameConflict, "...")`.
3. Apply only non-null fields from `UpdateProfileRequest` to the entity
   (partial update — null means "leave unchanged").
4. `DataNascita` is NOT updatable — ignore if present in request.
5. Persist via `IUserRepository.UpdateAsync`.
6. Return `Result.Ok(mappedDto)`.

### 6.8 `ChangePasswordHandler`

File: `MusicApp.Application/UseCases/Users/ChangePasswordHandler.cs`

Dependencies: `IUserRepository`, `IPasswordHasher`

Logic:
1. Load user by id.
2. Verify `PasswordOld` against stored hash. If false, return
   `Result.Fail(ErrorCodes.WrongPassword, "...")`.
3. Hash `PasswordNew`.
4. Update `user.PasswordHash` and persist.
5. Return `Result.Ok(true)`.

### 6.9 `DeleteMyAccountHandler`

File: `MusicApp.Application/UseCases/Users/DeleteMyAccountHandler.cs`

Dependencies: `IUserRepository`, `IPasswordHasher`, `ISessionRepository`

Logic:
1. Load user by id.
2. Verify `ConfermaPassword` against stored hash. If false, return
   `Result.Fail(ErrorCodes.WrongPassword, "...")`.
3. Soft-delete: set `user.IsActive = false`. Persist.
4. Revoke all sessions: call `ISessionRepository.DeleteAllForUserAsync(userId)`.
5. Return `Result.Ok(true)`.

Note: This handler must NEVER be callable when the user's role is Admin
(enforced at policy level in the controller — the handler itself does not
re-check role).

### 6.10 `GetPublicProfileHandler`

File: `MusicApp.Application/UseCases/Users/GetPublicProfileHandler.cs`

Dependencies: `IUserRepository`

Logic:
1. Load user by id. If null, return `Result.Fail(ErrorCodes.UserNotFound, "...")`.
2. Map to `PublicUserDto` (only public fields: Id, Nickname, Strumento, Descrizione).
3. Return `Result.Ok(dto)`.

---

## SECTION 7 — INFRASTRUCTURE LAYER: IMPLEMENTATIONS

### 7.1 `UserRepository`

File: `MusicApp.Infrastructure/Repositories/UserRepository.cs`

Implements `IUserRepository`. Injects `AppDbContext`.

All read queries that return a single user must include `.Include(u => u.Role)`
and `.Include(u => u.Plan)` so that `Role.Nome` and `Plan.Nome` are available
for DTO mapping. Do not include navigation properties beyond these two for
profile queries.

`UpdateAsync` must call `context.Users.Update(user)` then
`context.SaveChangesAsync()` and return the updated entity.

### 7.2 `SessionRepository`

File: `MusicApp.Infrastructure/Repositories/SessionRepository.cs`

Implements `ISessionRepository`.

`DeleteExpiredAsync` deletes all rows where `expires_at < DateTime.UtcNow`.
This is called once on application startup in `Program.cs` (fire-and-forget
inside a scoped block, not a hosted service — keep it simple in this phase).

### 7.3 `Argon2PasswordHasher`

File: `MusicApp.Infrastructure/Security/Argon2PasswordHasher.cs`

Implements `IPasswordHasher` using `Isopoh.Cryptography.Argon2`.

Configuration:
```csharp
var config = new Argon2Config
{
    Type           = Argon2Type.DataIndependentAddressing, // Argon2id
    Version        = Argon2Version.Nineteen,
    TimeCost       = 3,
    MemoryCost     = 65536,  // 64 MB
    Lanes          = 4,
    Threads        = 4,
    Password       = Encoding.UTF8.GetBytes(plaintext),
    HashLength     = 32
};
```

`Hash` returns the full encoded string (salt included in output).
`Verify` uses `Argon2.Verify(hash, plaintext)`.

### 7.4 `JwtService`

File: `MusicApp.Infrastructure/Security/JwtService.cs`

Implements `IJwtService`. Injects `IOptions<JwtOptions>`.

`GenerateAccessToken`:
- Algorithm: `HmacSha256`
- Claims to embed: `sub` (user.Id.ToString()), `email` (user.Email),
  `nickname` (user.Nickname), `role` (user.Role.Nome),
  `plan` (user.Plan.Nome), `is_active` (user.IsActive.ToString()),
  `jti` (Guid.NewGuid().ToString() — unique token ID for future revocation
  if needed)
- Expiry: `DateTime.UtcNow.AddMinutes(JwtOptions.AccessTokenExpiryMinutes)`
- Issuer and Audience from `JwtOptions`

`GenerateRefreshToken`:
- Return `Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))`
  — 64 bytes of cryptographic randomness encoded as Base64 (88-char string).
  This is the value stored in `sessions.token`.

### 7.5 `RefreshCookieHelper`

File: `MusicApp.Web/Infrastructure/RefreshCookieHelper.cs`

Implements `IRefreshCookieHelper`. This class lives in the Web project because
it depends on `Microsoft.AspNetCore.Http.HttpResponse`.

Cookie name constant: `private const string CookieName = "refresh_token";`

`SetRefreshCookie`:
```csharp
response.Cookies.Append(CookieName, token, new CookieOptions
{
    HttpOnly  = true,
    Secure    = true,
    SameSite  = SameSiteMode.Strict,
    Expires   = new DateTimeOffset(expiresAt),
    Path      = "/api/auth"   // scope cookie to auth routes only
});
```

`ClearRefreshCookie`:
```csharp
response.Cookies.Delete(CookieName, new CookieOptions
{
    HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict,
    Path = "/api/auth"
});
```

`ReadRefreshToken`:
```csharp
return request.Cookies.TryGetValue(CookieName, out var token) ? token : null;
```

---

## SECTION 8 — WEB LAYER: OPTIONS & CONFIGURATION

### 8.1 `JwtOptions`

File: `MusicApp.Web/Options/JwtOptions.cs`

```csharp
public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer                 { get; set; } = string.Empty;
    public string Audience               { get; set; } = string.Empty;
    public string Secret                 { get; set; } = string.Empty;
    public int    AccessTokenExpiryMinutes  { get; set; } = 15;
    public int    RefreshTokenExpiryDays    { get; set; } = 30;
}
```

Bind in `Program.cs`:
```csharp
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
```

### 8.2 Application DI extension

File: `MusicApp.Application/DependencyInjection.cs`

```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
    // Register all handlers as Scoped
    services.AddScoped<RegisterUserHandler>();
    services.AddScoped<LoginUserHandler>();
    services.AddScoped<RefreshTokenHandler>();
    services.AddScoped<LogoutHandler>();
    services.AddScoped<LogoutAllHandler>();
    services.AddScoped<GetMyProfileHandler>();
    services.AddScoped<UpdateMyProfileHandler>();
    services.AddScoped<ChangePasswordHandler>();
    services.AddScoped<DeleteMyAccountHandler>();
    services.AddScoped<GetPublicProfileHandler>();
    return services;
}
```

### 8.3 Infrastructure DI extension

File: `MusicApp.Infrastructure/DependencyInjection.cs`

```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    // DbContext already registered in Program.cs from Phase 0 — do not re-register
    services.AddScoped<IUserRepository, UserRepository>();
    services.AddScoped<ISessionRepository, SessionRepository>();
    services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
    services.AddScoped<IJwtService, JwtService>();
    return services;
}
```

---

## SECTION 9 — WEB LAYER: AUTHORIZATION POLICIES

Only the policies actually consumed by endpoints in this phase are registered
here. Policies for other roles (IsAdmin, IsTeacher, IsPro) and all
resource-based handlers (OwnerOrAdmin, TeacherOwnsSlot, etc.) are NOT
registered in this phase — they will be added in the phases that introduce
the endpoints requiring them.

Register the following in `Program.cs`:

```csharp
builder.Services.AddAuthorization(options =>
{
    // Default policy — applies to every [Authorize] attribute with no
    // explicit policy name. Requires a valid JWT AND is_active = true.
    // A user whose account has been soft-deleted (is_active = false) will
    // have is_active = "False" embedded in their JWT claims and will be
    // rejected here on every subsequent request after the token is issued.
    // Existing tokens remain signature-valid until their 15-min expiry —
    // this is an accepted tradeoff; no per-request DB lookup is added.
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim("is_active", "True")
        .Build();

    // CanDeleteOwnAccount — used only by DELETE /api/users/me.
    // Blocks Admin users from self-deleting via this endpoint.
    // (Admins are deleted by other Admins via /api/admin/users/{id},
    // implemented in Phase 8.)
    options.AddPolicy("CanDeleteOwnAccount", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .RequireAssertion(ctx =>
                  !ctx.User.HasClaim("role", "Admin")));
});
```

---

## SECTION 10 — WEB LAYER: CONTROLLERS

### 10.1 General controller conventions

- All API controllers inherit from `ControllerBase` and carry `[ApiController]`
  and `[Route("api/[controller]")]`.
- All endpoints return `IActionResult`.
- On validation failure, FluentValidation is wired to return `400 Bad Request`
  automatically via `AddFluentValidationAutoValidation()` in `Program.cs`.
  Do not duplicate manual validation in controller actions.
- On `Result.IsSuccess == false`, map `ErrorCode` to HTTP status as specified
  per endpoint below.
- Extract the current user's id from JWT claims with a helper:
  `int userId = int.Parse(User.FindFirstValue("sub")!);`

### 10.2 `AuthController`

File: `MusicApp.Web/Controllers/AuthController.cs`
Route prefix: `/api/auth`

---

#### `POST /api/auth/register` — anonymous

1. Validate request body as `RegisterRequest` (FluentValidation auto-runs).
2. Call `RegisterUserHandler.HandleAsync(request)`.
3. On failure:
   - `EMAIL_ALREADY_EXISTS` → 409 Conflict
   - `NICKNAME_ALREADY_EXISTS` → 409 Conflict
   - `UNDER_AGE` → 422 Unprocessable Entity
4. On success: call `IRefreshCookieHelper.SetRefreshCookie(...)`.
   Return `200 OK` with `AuthResponse` body.

---

#### `POST /api/auth/login` — anonymous

1. Validate `LoginRequest`.
2. Call `LoginUserHandler.HandleAsync(request)`.
3. On failure:
   - `INVALID_CREDENTIALS` → 401 Unauthorized (same message regardless of
     whether email or password was wrong — do NOT leak which field failed)
   - `ACCOUNT_INACTIVE` → 403 Forbidden
4. On success: set cookie, return `200 OK` with `AuthResponse`.

---

#### `POST /api/auth/refresh` — anonymous

1. Read refresh token from cookie via `IRefreshCookieHelper.ReadRefreshToken`.
   If null (no cookie present), return `401 Unauthorized` immediately.
2. Call `RefreshTokenHandler.HandleAsync(tokenString)`.
3. On failure:
   - `INVALID_REFRESH_TOKEN` → 401 Unauthorized. Also call
     `IRefreshCookieHelper.ClearRefreshCookie` before returning.
   - `EXPIRED_REFRESH_TOKEN` → 401 Unauthorized. Clear cookie.
   - `ACCOUNT_INACTIVE` → 403 Forbidden. Clear cookie.
4. On success: overwrite the cookie with the new refresh token via
   `SetRefreshCookie`, return `200 OK` with new `AuthResponse`.

---

#### `POST /api/auth/logout` — `[Authorize]` (default policy)

1. Read refresh token from cookie. If null, return `204 No Content` (idempotent).
2. Call `LogoutHandler.HandleAsync(tokenString)`.
3. Call `IRefreshCookieHelper.ClearRefreshCookie`.
4. Return `204 No Content`.

---

#### `POST /api/auth/logout-all` — `[Authorize]` (default policy)

1. Extract `userId` from JWT claim `sub`.
2. Call `LogoutAllHandler.HandleAsync(userId)`.
3. Call `IRefreshCookieHelper.ClearRefreshCookie` (clears the current device).
4. Return `204 No Content`.

---

### 10.3 `UsersController`

File: `MusicApp.Web/Controllers/UsersController.cs`
Route prefix: `/api/users`

---

#### `GET /api/users/me` — `[Authorize]`

1. Extract `userId`.
2. Call `GetMyProfileHandler.HandleAsync(userId)`.
3. On failure: `USER_NOT_FOUND` → 404 Not Found.
4. On success: return `200 OK` with `UserProfileDto`.

---

#### `PATCH /api/users/me` — `[Authorize]`

1. Validate `UpdateProfileRequest`.
2. Extract `userId`.
3. Call `UpdateMyProfileHandler.HandleAsync(userId, request)`.
4. On failure:
   - `NICKNAME_CONFLICT` → 409 Conflict
   - `USER_NOT_FOUND` → 404 Not Found
5. On success: return `200 OK` with updated `UserProfileDto`.

---

#### `PATCH /api/users/me/password` — `[Authorize]`

1. Validate `ChangePasswordRequest`.
2. Extract `userId`.
3. Call `ChangePasswordHandler.HandleAsync(userId, request)`.
4. On failure:
   - `WRONG_PASSWORD` → 400 Bad Request with error message
   - `USER_NOT_FOUND` → 404 Not Found
5. On success: return `204 No Content`.
   Do NOT invalidate existing sessions — the user stays logged in on all devices.
   (Forced re-login on password change is deferred to a later decision.)

---

#### `DELETE /api/users/me` — `[Authorize(Policy = "CanDeleteOwnAccount")]`

1. Validate `DeleteAccountRequest`.
2. Extract `userId` from JWT.
3. As an extra guard (defense in depth), verify that `userId` parsed from
   the JWT `sub` claim matches the resource being deleted. Since this endpoint
   always deletes "me", this is implicit — no additional check needed.
4. Call `DeleteMyAccountHandler.HandleAsync(userId, request)`.
5. On failure:
   - `WRONG_PASSWORD` → 400 Bad Request
   - `USER_NOT_FOUND` → 404 Not Found
6. On success: call `IRefreshCookieHelper.ClearRefreshCookie`.
   Return `204 No Content`.

---

#### `GET /api/users/{id}/public` — no `[Authorize]` (anonymous)

1. Call `GetPublicProfileHandler.HandleAsync(id)`.
2. On failure: `USER_NOT_FOUND` → 404 Not Found.
3. On success: return `200 OK` with `PublicUserDto`.

Note: expose only `Id`, `Nickname`, `Strumento`, `Descrizione`. Do NOT
expose email, real name, is_active, or any internal field.

---

## SECTION 11 — PROGRAM.CS UPDATES

Add the following to `Program.cs` after Phase 0 skeleton, in order:

```csharp
// 1. Bind options
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

// 2. Register application and infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// 3. Register Web-layer services
builder.Services.AddScoped<IRefreshCookieHelper, RefreshCookieHelper>();

// 4. Configure JWT Bearer (replace the stub from Phase 0 with the full config)
var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtOptions.Issuer,
            ValidAudience            = jwtOptions.Audience,
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew                = TimeSpan.Zero  // no tolerance — 15 min means 15 min
        };
    });

// 5. Authorization policies (as defined in Section 9)
builder.Services.AddAuthorization(options => { /* ... */ });

// 6. FluentValidation auto-validation
builder.Services.AddFluentValidationAutoValidation();

// 7. Controllers (if not already added)
builder.Services.AddControllers();

// --- after app.Build() ---

// 8. Purge expired sessions at startup (fire-and-forget, scoped)
using (var scope = app.Services.CreateScope())
{
    var sessionRepo = scope.ServiceProvider
        .GetRequiredService<ISessionRepository>();
    await sessionRepo.DeleteExpiredAsync();
}

// 9. Seed data (from Phase 0 — already present, keep as-is)

// 10. Standard middleware pipeline order (MUST be in this exact order)
app.UseHttpsRedirection();
app.UseAuthentication();   // must come before UseAuthorization
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();
app.Run();
```

---

## SECTION 12 — ERROR RESPONSE SHAPE

All error responses from API controllers must use a consistent JSON envelope.
Create `MusicApp.Web/Models/ApiError.cs`:

```csharp
public record ApiError(string Code, string Message);
```

Controllers return errors as:
```csharp
return StatusCode(statusCode, new ApiError(result.ErrorCode!, result.ErrorMessage!));
```

Example 401 body:
```json
{ "code": "INVALID_CREDENTIALS", "message": "Email o password non validi." }
```

FluentValidation failures are handled automatically by `[ApiController]` and
return the standard ASP.NET `ValidationProblemDetails` shape — do not override
this behavior.

---

## SECTION 13 — SECURITY INVARIANTS

The following invariants must hold throughout this phase. Verify each one
after implementation:

1. **Timing-safe login:** the time elapsed between "email not found" and
   "wrong password" responses must not be measurably different from the
   client's perspective. Both code paths call `IPasswordHasher.Verify`
   (even on a dummy hash if the email is not found) before returning.
   Implement this in `LoginUserHandler`: if user is null, call
   `passwordHasher.Verify(request.Password, "$argon2id$v=19$m=65536,t=3,p=4$dummysalt$dummyhash")`
   — the verify will fail fast but the timing difference is negligible and
   prevents user enumeration via response time.

2. **Refresh token never in response body:** the refresh token string is
   only ever passed to `IRefreshCookieHelper.SetRefreshCookie` and never
   included in any JSON response body.

3. **ClockSkew = TimeSpan.Zero:** access tokens expire strictly at 15 min,
   no tolerance window.

4. **Cookie path scoped to /api/auth:** the refresh token cookie is not sent
   on every request — only on requests to `/api/auth/*`. This limits its
   exposure surface.

5. **Soft-delete is not hard-delete:** `DELETE /api/users/me` sets
   `is_active = false`. It does NOT delete the database row. The user's data
   is preserved for admin review and potential reactivation.

6. **is_active checked on every authenticated request:** the `is_active`
   claim is embedded in the JWT at login/register time. A soft-deleted user
   (is_active = false) will have `is_active = "False"` in their existing JWT
   claims and will be rejected by the default policy on every subsequent
   authenticated request. Tokens already in circulation remain
   signature-valid until their 15-min natural expiry — accept this window
   as a known and deliberate tradeoff. Do NOT add a per-request DB lookup
   to recheck `is_active` at the middleware level.

---

## SECTION 14 — VALIDATION CHECKLIST

Before declaring Phase 1 complete, verify every item:

- [ ] `dotnet build` passes with 0 errors across all 4 projects
- [ ] `POST /api/auth/register` with valid payload returns 200 + JWT + cookie
- [ ] `POST /api/auth/register` with duplicate email returns 409
- [ ] `POST /api/auth/register` with duplicate nickname returns 409
- [ ] `POST /api/auth/register` with age < 18 returns 422
- [ ] `POST /api/auth/login` with wrong password returns 401 (not 400)
- [ ] `POST /api/auth/login` for banned user returns 403
- [ ] `POST /api/auth/refresh` with valid cookie returns 200 + new JWT +
      rotated cookie (old token is deleted from sessions table)
- [ ] `POST /api/auth/refresh` with expired cookie returns 401 + cleared cookie
- [ ] `POST /api/auth/refresh` with tampered/unknown token returns 401
- [ ] `POST /api/auth/logout` deletes the session row from DB
- [ ] `POST /api/auth/logout-all` deletes ALL session rows for the user
- [ ] `GET /api/users/me` with valid JWT returns UserProfileDto
- [ ] `GET /api/users/me` without JWT returns 401
- [ ] `PATCH /api/users/me` with duplicate nickname returns 409
- [ ] `PATCH /api/users/me/password` with wrong old password returns 400
- [ ] `DELETE /api/users/me` sets is_active = false (row still exists in DB)
- [ ] `DELETE /api/users/me` with wrong confirmation password returns 400
- [ ] `DELETE /api/users/me` called by a user with role Admin returns 403
- [ ] `GET /api/users/{id}/public` returns only Id, Nickname, Strumento,
      Descrizione — no email, no real name
- [ ] `GET /api/users/{id}/public` for nonexistent id returns 404
- [ ] No endpoint reveals whether an email address is registered except
      through successful login
- [ ] Running startup session purge (`DeleteExpiredAsync`) does not throw
      when sessions table is empty

---

## SECTION 15 — OUT OF SCOPE FOR THIS PHASE

Do NOT implement any of the following. They belong to later phases:

- Exercise endpoints, attempt saving, level progression (Phase 2)
- Forum posts, comments, votes (Phase 3)
- Teacher profiles, slots, bookings, ratings (Phase 4)
- Stripe payments, subscriptions, bundles (Phase 5)
- Chat endpoints (Phase 6)
- Notification system (Phase 7)
- Admin dashboard, moderation endpoints, user ban/unban (Phase 8)
- SignalR real-time hub (Phase 9)
- Resource-based authorization handlers (OwnerOrAdmin, TeacherOwnsSlot,
  TeacherOwnsBooking, StudentOwnsBooking, BookingParticipant,
  TeacherManagesStudent, RatingWindow) — implemented in the phases that
  introduce the endpoints requiring them
- Email verification or password-reset-by-email flow — not planned for this app
- Rate limiting on auth endpoints — deferred
- Background hosted service for session cleanup — `DeleteExpiredAsync` is
  called once at startup only in this phase

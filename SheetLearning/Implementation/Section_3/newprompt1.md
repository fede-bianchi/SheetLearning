# SYSTEM PROMPT — Phase 3: Forum (Posts, Comments, Votes)
# Music Learning App

You are a senior .NET backend engineer continuing work on a solution built
across Phase 0 (infrastructure + database), Phase 1 (authentication + user
profile), and Phase 2 (exercises + progression). All three phases are complete
and must not be modified.

Your scope in this phase:
- Application layer: new repository interfaces, use-case handlers, DTOs,
  FluentValidation validators for the 11 forum endpoints listed below
- Application layer: `OwnerOrAdmin` resource-based authorization requirement
  and handler — first resource-based policy introduced in this project
- Infrastructure layer: implementations of all new repository interfaces
- Web layer: 3 new API controllers (`PostsController`, `CommentsController`,
  `VotesController`); registration of the `OwnerOrAdmin` policy and handler
- Program.cs: register all new services introduced in this phase only

No frontend. Backend API only.

---

## SECTION 1 — PREREQUISITES & CONSTRAINTS

**Phases 0–2 outputs (already exist, do not touch):**
- All 22 EF Core entities, configurations, migrations, seed data
- All Phase 1 interfaces, handlers, DTOs (auth + user profile)
- All Phase 2 interfaces, handlers, DTOs (exercises + progression)
- `AuthController`, `UsersController`, `ExerciseTypesController`,
  `LevelsController`, `AttemptsController`
- `Result<T>`, `ErrorCodes` (with Phase 1 and 2 codes), `ApiError`,
  `JwtOptions`, `PublicUserDto`
- `DefaultPolicy` and `CanDeleteOwnAccount` registered in `Program.cs`

**New ErrorCodes to append** to the existing `ErrorCodes` static class in
`MusicApp.Application/Common/Result.cs`. Do not replace the class:

```csharp
// Phase 3
public const string PostNotFound       = "POST_NOT_FOUND";
public const string CommentNotFound    = "COMMENT_NOT_FOUND";
public const string PostDeleted        = "POST_DELETED";
public const string CommentDeleted     = "COMMENT_DELETED";
public const string Forbidden          = "FORBIDDEN";
public const string VoteTargetNotFound = "VOTE_TARGET_NOT_FOUND";
public const string InvalidTargetType  = "INVALID_TARGET_TYPE";
```

---

## SECTION 2 — DOMAIN RULES

### 2.1 Soft-delete display contract

`posts` and `comments` are never physically deleted by their authors or
admins in this phase. Soft-delete sets `is_deleted = true` and
`deleted_at = DateTime.UtcNow`.

Display rules for soft-deleted content in API responses:
- Soft-deleted posts appear in list and detail endpoints with
  `Contenuto = "[contenuto rimosso]"` and `Titolo = "[post rimosso]"`.
  The `Autore` field is preserved. The post is NOT excluded from lists.
- Soft-deleted comments appear in the comment tree with
  `Contenuto = "[commento rimosso]"`. The `Autore` field is preserved.
  Child replies to a deleted comment are still rendered — the thread
  structure must not be broken.
- `IsDeleted = true` is included in every response DTO so the client can
  render deleted content differently if desired.

Rationale: hiding deleted nodes would orphan visible replies, breaking
thread coherence. The content is redacted, not the node.

### 2.2 Vote aggregation

`UpvoteCount` and `DownvoteCount` are computed at query time by counting
rows in the `votes` table where `target_type = 'post'` (or `'comment'`)
and `target_id = resource.Id`. They are NEVER stored as columns — always
computed dynamically.

`UserVote` is the `voto` value (`"upvote"` or `"downvote"`) of the
requesting user's vote row, if one exists. For unauthenticated requests
(anonymous GET endpoints), `UserVote = null`.

### 2.3 Comment tree structure

`GET /api/posts/{postId}/comments` returns the FULL threaded tree, not just
top-level comments. The tree is built server-side from a flat DB query:
- Load all non-deleted AND deleted comments for the post (all nodes must
  be present to preserve thread structure per rule 2.1).
- Build the tree recursively in memory: each `CommentDto` has a `Risposte`
  collection containing its direct children, which in turn have their own
  `Risposte`.
- Root nodes are comments where `ParentCommentId == null`.
- Order: within each level, sort by `CreatedAt` ascending (oldest first).

`GET /api/posts/{id}` (post detail) includes only TOP-LEVEL comments
(those with `ParentCommentId == null`) without their nested replies.
The client uses the separate `GET /api/posts/{postId}/comments` endpoint
to retrieve the full tree.

### 2.4 Pagination for post list

`GET /api/posts` supports:
- `Page`: 1-based. Default 1.
- `PageSize`: default 20, max 50. Values above 50 are clamped to 50.
- `OrderBy`: `"recenti"` sorts by `CreatedAt` descending.
  `"votati"` sorts by net vote score (upvotes − downvotes) descending,
  then by `CreatedAt` descending as tiebreaker.

The response must include a `TotalCount` field (total posts matching the
filter, before pagination) so the client can render pagination controls.

### 2.5 Vote upsert semantics

`POST /api/votes`:
- If the user has no existing vote on the target: create a new vote row.
- If the user already voted the SAME direction: treat as idempotent —
  return `200 OK` without error (do not create a duplicate).
- If the user already voted the OPPOSITE direction: REPLACE the existing
  vote (update the `voto` field). Do not create a second row.

`DELETE /api/votes`:
- If the user has no vote on the target: return `204 No Content`
  (idempotent — not an error).
- If the user has a vote: delete it, return `204 No Content`.

### 2.6 OwnerOrAdmin authorization contract

Endpoints marked `OwnerOrAdmin` enforce this rule:
> The requesting user must either be the author of the resource
> (post.UserId == JWT sub, or comment.UserId == JWT sub) OR have
> role = "Admin".

This is implemented as a proper ASP.NET Core resource-based authorization
handler, not as inline controller logic. The contract is:
1. The controller loads the resource from the DB.
2. The controller calls `IAuthorizationService.AuthorizeAsync(User, resource, "OwnerOrAdmin")`.
3. If authorization fails → return `403 Forbidden`.
4. If the resource is soft-deleted and the requester is not Admin:
   - For `PUT` (update): return `400 Bad Request` with `POST_DELETED` or
     `COMMENT_DELETED` (cannot edit deleted content).
   - For `DELETE`: return `204 No Content` (idempotent — already deleted).

### 2.7 ParentComment validation

When `CreateCommentRequest.ParentCommentId` is not null:
- The parent comment must exist.
- The parent comment must belong to the same post (`PostId` must match).
- A reply to a soft-deleted comment IS allowed (the thread must remain
  intact).
- Maximum nesting depth is NOT enforced server-side — any depth is valid.

---

## SECTION 3 — RESOURCE-BASED AUTHORIZATION: OwnerOrAdmin

This is the first resource-based authorization policy in this project.
Implement the full ASP.NET Core resource-based authorization pattern.

### 3.1 `IOwnedResource` interface

File: `MusicApp.Application/Authorization/IOwnedResource.cs`

```csharp
namespace MusicApp.Application.Authorization;

public interface IOwnedResource
{
    int OwnerId { get; }
}
```

### 3.2 `OwnerOrAdminRequirement`

File: `MusicApp.Application/Authorization/OwnerOrAdminRequirement.cs`

```csharp
using Microsoft.AspNetCore.Authorization;

namespace MusicApp.Application.Authorization;

public class OwnerOrAdminRequirement : IAuthorizationRequirement { }
```

### 3.3 `OwnerOrAdminHandler`

File: `MusicApp.Infrastructure/Authorization/OwnerOrAdminHandler.cs`

Lives in Infrastructure because it reads JWT claims from `ClaimsPrincipal`,
which is an ASP.NET concern tolerated at the Infrastructure boundary.

```csharp
using Microsoft.AspNetCore.Authorization;
using MusicApp.Application.Authorization;

namespace MusicApp.Infrastructure.Authorization;

public class OwnerOrAdminHandler
    : AuthorizationHandler<OwnerOrAdminRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdminRequirement requirement,
        IOwnedResource resource)
    {
        var sub  = context.User.FindFirst("sub")?.Value;
        var role = context.User.FindFirst("role")?.Value;

        if (role == "Admin")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (sub != null && int.TryParse(sub, out var userId)
            && userId == resource.OwnerId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### 3.4 Registration in Program.cs

Add the following — do not touch existing policy registrations:

```csharp
// OwnerOrAdmin resource-based handler (Phase 3)
builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrAdminHandler>();
```

Add to the existing `AddAuthorization` options block:

```csharp
options.AddPolicy("OwnerOrAdmin", policy =>
    policy.RequireAuthenticatedUser()
          .RequireClaim("is_active", "True")
          .AddRequirements(new OwnerOrAdminRequirement()));
```

---

## SECTION 4 — APPLICATION LAYER: INTERFACES

Create in `MusicApp.Application/Interfaces/`.

### 4.1 `IPostRepository`

```csharp
Task<(IReadOnlyList<Post> Posts, int TotalCount)> GetPagedAsync(
    int page, int pageSize, string orderBy);

Task<Post?> GetByIdAsync(int id);

// Includes User navigation (for author mapping)
Task<Post?> GetByIdWithAuthorAsync(int id);

Task<Post> CreateAsync(Post post);
Task<Post> UpdateAsync(Post post);

// Soft-delete: sets is_deleted = true, deleted_at = UtcNow
Task SoftDeleteAsync(Post post);
```

### 4.2 `ICommentRepository`

```csharp
// Returns ALL comments for a post (including soft-deleted) with User nav.
// Ordered by CreatedAt ascending.
Task<IReadOnlyList<Comment>> GetAllByPostAsync(int postId);

// Returns only top-level comments (ParentCommentId == null) for a post.
// Includes User nav. Ordered by CreatedAt ascending.
Task<IReadOnlyList<Comment>> GetTopLevelByPostAsync(int postId);

Task<Comment?> GetByIdAsync(int id);
Task<Comment?> GetByIdWithAuthorAsync(int id);

Task<Comment> CreateAsync(Comment comment);
Task<Comment> UpdateAsync(Comment comment);
Task SoftDeleteAsync(Comment comment);
```

### 4.3 `IVoteRepository`

```csharp
Task<Vote?> GetByUserAndTargetAsync(
    int userId, string targetType, int targetId);

// Returns vote counts for a single target.
Task<(int Upvotes, int Downvotes)> GetCountsAsync(
    string targetType, int targetId);

// Batch: returns vote counts for multiple targets of the same type.
// Key: targetId. Value: (Upvotes, Downvotes).
Task<Dictionary<int, (int Upvotes, int Downvotes)>> GetCountsBatchAsync(
    string targetType, IEnumerable<int> targetIds);

// Returns the votes cast by a given user for a list of target IDs of the
// same type. Key: targetId. Value: "upvote" or "downvote".
Task<Dictionary<int, string>> GetUserVotesBatchAsync(
    int userId, string targetType, IEnumerable<int> targetIds);

Task<Vote> CreateAsync(Vote vote);
Task<Vote> UpdateAsync(Vote vote);
Task DeleteAsync(Vote vote);
```

---

## SECTION 5 — APPLICATION LAYER: DTOs

Add to `MusicApp.Application/DTOs/`. Do not modify existing files.

### 5.1 `PostSummaryDto`

```csharp
public record PostSummaryDto(
    int          Id,
    string       Titolo,
    string       Contenuto,      // redacted if IsDeleted
    PublicUserDto Autore,
    int          UpvoteCount,
    int          DownvoteCount,
    string?      UserVote,        // null if anonymous or no vote
    int          CommentCount,    // total non-deleted comments on the post
    bool         IsDeleted,
    DateTime     CreatedAt,
    DateTime     UpdatedAt
);
```

### 5.2 `PostListResponse`

```csharp
public record PostListResponse(
    IReadOnlyList<PostSummaryDto> Posts,
    int TotalCount,
    int Page,
    int PageSize
);
```

### 5.3 `PostDetailDto`

```csharp
public record PostDetailDto(
    int                          Id,
    string                       Titolo,
    string                       Contenuto,
    PublicUserDto                Autore,
    int                          UpvoteCount,
    int                          DownvoteCount,
    string?                      UserVote,
    bool                         IsDeleted,
    DateTime                     CreatedAt,
    DateTime                     UpdatedAt,
    IReadOnlyList<CommentDto>    CommentiTopLevel  // top-level only, no replies
);
```

### 5.4 `CommentDto`

Recursive — `Risposte` contains children of the same type.

```csharp
public record CommentDto(
    int                       Id,
    int                       PostId,
    int?                      ParentCommentId,
    string                    Contenuto,       // redacted if IsDeleted
    PublicUserDto             Autore,
    int                       UpvoteCount,
    int                       DownvoteCount,
    string?                   UserVote,
    bool                      IsDeleted,
    DateTime                  CreatedAt,
    DateTime                  UpdatedAt,
    IReadOnlyList<CommentDto> Risposte         // direct children, recursively built
);
```

### 5.5 Request DTOs

```csharp
public record CreatePostRequest(
    string Titolo,
    string Contenuto
);

public record UpdatePostRequest(
    string Titolo,
    string Contenuto
);

public record CreateCommentRequest(
    string Contenuto,
    int?   ParentCommentId
);

public record UpdateCommentRequest(
    string Contenuto
);

public record VoteRequest(
    string TargetType,   // "post" / "comment"
    int    TargetId,
    string Voto          // "upvote" / "downvote" — not used by DELETE but
                         // included for symmetry; DELETE ignores Voto
);
```

### 5.6 `PostListQuery` (query string binding)

```csharp
public record PostListQuery(
    int    Page     = 1,
    int    PageSize = 20,
    string OrderBy  = "recenti"   // "recenti" | "votati"
);
```

---

## SECTION 6 — APPLICATION LAYER: VALIDATORS

Add to `MusicApp.Application/Validators/`.

### 6.1 `CreatePostRequestValidator`

| Field | Rules |
|---|---|
| `Titolo` | `NotEmpty`, `MaxLength(200)` |
| `Contenuto` | `NotEmpty`, `MaxLength(10000)` |

### 6.2 `UpdatePostRequestValidator`

Same rules as `CreatePostRequestValidator`.

### 6.3 `CreateCommentRequestValidator`

| Field | Rules |
|---|---|
| `Contenuto` | `NotEmpty`, `MaxLength(5000)` |
| `ParentCommentId` | When not null: `GreaterThan(0)` |

### 6.4 `UpdateCommentRequestValidator`

| Field | Rules |
|---|---|
| `Contenuto` | `NotEmpty`, `MaxLength(5000)` |

### 6.5 `VoteRequestValidator`

| Field | Rules |
|---|---|
| `TargetType` | `NotEmpty`, must be `"post"` or `"comment"` |
| `TargetId` | `GreaterThan(0)` |
| `Voto` | `NotEmpty`, must be `"upvote"` or `"downvote"` |

---

## SECTION 7 — APPLICATION LAYER: HELPERS

### 7.1 `SoftDeleteContentHelper`

File: `MusicApp.Application/Helpers/SoftDeleteContentHelper.cs`

A static class used by all handlers to apply the redaction contract
(domain rule 2.1) when mapping entities to DTOs. Centralizes the
"is_deleted → replace content" logic.

```csharp
public static class SoftDeleteContentHelper
{
    public const string DeletedPostTitolo   = "[post rimosso]";
    public const string DeletedPostContenuto = "[contenuto rimosso]";
    public const string DeletedCommentContenuto = "[commento rimosso]";

    public static string ResolvePostTitolo(Post post)
        => post.IsDeleted ? DeletedPostTitolo : post.Titolo;

    public static string ResolvePostContenuto(Post post)
        => post.IsDeleted ? DeletedPostContenuto : post.Contenuto;

    public static string ResolveCommentContenuto(Comment comment)
        => comment.IsDeleted ? DeletedCommentContenuto : comment.Contenuto;
}
```

### 7.2 `CommentTreeBuilder`

File: `MusicApp.Application/Helpers/CommentTreeBuilder.cs`

Builds the recursive `CommentDto` tree from a flat list of comments.
Receives the flat list and a function to map a single `Comment` to
`CommentDto` (without children) so it stays decoupled from DTO concerns.

```csharp
public static class CommentTreeBuilder
{
    public static IReadOnlyList<CommentDto> Build(
        IReadOnlyList<Comment> flat,
        Func<Comment, IReadOnlyList<CommentDto>, CommentDto> mapFn)
    {
        // Group by ParentCommentId
        var byParent = flat.ToLookup(c => c.ParentCommentId);

        // Recursive builder
        IReadOnlyList<CommentDto> BuildChildren(int? parentId)
        {
            var children = byParent[parentId]
                .OrderBy(c => c.CreatedAt)
                .ToList();

            return children
                .Select(c => mapFn(c, BuildChildren(c.Id)))
                .ToList();
        }

        return BuildChildren(null);  // start from root (ParentCommentId = null)
    }
}
```

---

## SECTION 8 — APPLICATION LAYER: USE-CASE HANDLERS

All handlers live in `MusicApp.Application/UseCases/Forum/`.

### 8.1 `GetPostsHandler`

Dependencies: `IPostRepository`, `IVoteRepository`

Accepts: `PostListQuery query`, `int? currentUserId` (null if anonymous)

Logic:
1. Clamp `query.PageSize` to max 50.
2. Call `IPostRepository.GetPagedAsync(query.Page, clampedPageSize, query.OrderBy)`.
3. Extract post IDs from the returned page.
4. Batch-fetch vote counts: `IVoteRepository.GetCountsBatchAsync("post", postIds)`.
5. If `currentUserId != null`: batch-fetch user votes:
   `IVoteRepository.GetUserVotesBatchAsync(currentUserId.Value, "post", postIds)`.
6. Map each post to `PostSummaryDto` using `SoftDeleteContentHelper`.
   `CommentCount` is NOT included in the paged query — set it to 0 in this
   handler. The list view does not need comment counts (performance tradeoff;
   this can be added in Phase 9 if needed).
7. Return `Result.Ok(new PostListResponse(...))`.

### 8.2 `GetPostDetailHandler`

Dependencies: `IPostRepository`, `ICommentRepository`, `IVoteRepository`

Accepts: `int postId`, `int? currentUserId`

Logic:
1. Load post with author: `IPostRepository.GetByIdWithAuthorAsync(postId)`.
   If null, return `Result.Fail(ErrorCodes.PostNotFound, "...")`.
2. Fetch vote counts for the post.
3. Fetch user vote for the post (if authenticated).
4. Fetch top-level comments: `ICommentRepository.GetTopLevelByPostAsync(postId)`.
   This returns comments with `ParentCommentId == null` only — no tree building
   needed here.
5. For each top-level comment, fetch vote counts (batch).
6. Fetch user votes for top-level comments (batch, if authenticated).
7. Map to `PostDetailDto`. `CommentiTopLevel` contains mapped `CommentDto`
   objects with empty `Risposte` (top-level only, no recursion).
8. Return `Result.Ok(dto)`.

### 8.3 `CreatePostHandler`

Dependencies: `IPostRepository`

Accepts: `int userId`, `CreatePostRequest request`

Logic:
1. Create `Post` entity: `UserId = userId`, `Titolo`, `Contenuto`,
   `IsDeleted = false`, `CreatedAt = UpdatedAt = DateTime.UtcNow`.
2. Persist via `IPostRepository.CreateAsync`.
3. Map to `PostSummaryDto` with `UpvoteCount = 0`, `DownvoteCount = 0`,
   `UserVote = null`, `CommentCount = 0`, `IsDeleted = false`.
4. Return `Result.Ok(dto)`.

### 8.4 `UpdatePostHandler`

Dependencies: `IPostRepository`

Accepts: `int postId`, `UpdatePostRequest request`

Note: ownership/admin check is performed by the controller before calling
this handler (see Section 10). The handler trusts that authorization passed.

Logic:
1. Load post: `IPostRepository.GetByIdAsync(postId)`.
   If null, return `Result.Fail(ErrorCodes.PostNotFound, "...")`.
2. If `post.IsDeleted`, return `Result.Fail(ErrorCodes.PostDeleted,
   "Cannot edit a deleted post.")`.
3. Update `Titolo`, `Contenuto`, `UpdatedAt = DateTime.UtcNow`.
4. Persist via `IPostRepository.UpdateAsync`.
5. Reload vote counts (the post may have votes from before the edit).
6. Map to `PostSummaryDto` and return `Result.Ok(dto)`.

### 8.5 `DeletePostHandler`

Dependencies: `IPostRepository`

Accepts: `int postId`

Note: authorization check performed by controller before calling.

Logic:
1. Load post. If null, return `Result.Fail(ErrorCodes.PostNotFound, "...")`.
2. If `post.IsDeleted`, return `Result.Ok(true)` (idempotent).
3. Call `IPostRepository.SoftDeleteAsync(post)`.
4. Return `Result.Ok(true)`.

### 8.6 `GetCommentsHandler`

Dependencies: `ICommentRepository`, `IVoteRepository`

Accepts: `int postId`, `int? currentUserId`

Logic:
1. Load ALL comments for the post (including soft-deleted) with author data:
   `ICommentRepository.GetAllByPostAsync(postId)`.
2. If the list is empty AND the post does not exist, that's acceptable —
   the handler does not need to verify post existence (saves a DB round-trip).
   Return empty list in that case; the client will have already validated the
   post exists from the detail endpoint.
3. Extract all comment IDs.
4. Batch-fetch vote counts: `IVoteRepository.GetCountsBatchAsync("comment", commentIds)`.
5. If `currentUserId != null`: batch-fetch user votes for all comments.
6. Define the per-comment map function (used by `CommentTreeBuilder`):
   maps a `Comment` + its `IReadOnlyList<CommentDto>` children to a `CommentDto`,
   applying `SoftDeleteContentHelper.ResolveCommentContenuto`.
7. Call `CommentTreeBuilder.Build(flat, mapFn)`.
8. Return `Result.Ok(tree)`.

### 8.7 `CreateCommentHandler`

Dependencies: `ICommentRepository`, `IPostRepository`

Accepts: `int userId`, `int postId`, `CreateCommentRequest request`

Logic:
1. Load post: `IPostRepository.GetByIdAsync(postId)`.
   If null, return `Result.Fail(ErrorCodes.PostNotFound, "...")`.
   Note: allow commenting on soft-deleted posts is intentionally NOT allowed.
   If `post.IsDeleted`, return `Result.Fail(ErrorCodes.PostDeleted,
   "Cannot comment on a deleted post.")`.
2. If `request.ParentCommentId != null`:
   - Load parent comment: `ICommentRepository.GetByIdAsync(request.ParentCommentId.Value)`.
   - If null, return `Result.Fail(ErrorCodes.CommentNotFound, "Parent comment not found.")`.
   - If `parentComment.PostId != postId`, return
     `Result.Fail(ErrorCodes.CommentNotFound, "Parent comment does not belong to this post.")`.
   - (A reply to a soft-deleted comment IS allowed — do not check IsDeleted.)
3. Create `Comment` entity and persist.
4. Reload with author navigation for response mapping.
5. Map to `CommentDto` with `UpvoteCount = 0`, `DownvoteCount = 0`,
   `UserVote = null`, `Risposte = []`.
6. Return `Result.Ok(dto)`.

### 8.8 `UpdateCommentHandler`

Dependencies: `ICommentRepository`

Accepts: `int commentId`, `UpdateCommentRequest request`

Note: authorization check performed by controller before calling.

Logic:
1. Load comment with author. If null, return
   `Result.Fail(ErrorCodes.CommentNotFound, "...")`.
2. If `comment.IsDeleted`, return `Result.Fail(ErrorCodes.CommentDeleted,
   "Cannot edit a deleted comment.")`.
3. Update `Contenuto`, `UpdatedAt = DateTime.UtcNow`. Persist.
4. Reload vote counts.
5. Map to `CommentDto` (with empty `Risposte`) and return `Result.Ok(dto)`.

### 8.9 `DeleteCommentHandler`

Dependencies: `ICommentRepository`

Accepts: `int commentId`

Note: authorization check performed by controller before calling.

Logic:
1. Load comment. If null, return `Result.Fail(ErrorCodes.CommentNotFound, "...")`.
2. If `comment.IsDeleted`, return `Result.Ok(true)` (idempotent).
3. Call `ICommentRepository.SoftDeleteAsync(comment)`.
4. Return `Result.Ok(true)`.

### 8.10 `UpsertVoteHandler`

Dependencies: `IVoteRepository`, `IPostRepository`, `ICommentRepository`

Accepts: `int userId`, `VoteRequest request`

Logic:
1. Validate `TargetType` is `"post"` or `"comment"`. (FluentValidation
   already checked this — but the handler must re-check for safety since
   TargetType is used in a string-typed ENUM column.)
2. Verify the target exists:
   - If `TargetType == "post"`: load post by `TargetId`. If null or
     `IsDeleted`, return `Result.Fail(ErrorCodes.VoteTargetNotFound, "...")`.
   - If `TargetType == "comment"`: load comment by `TargetId`. If null or
     `IsDeleted`, return `Result.Fail(ErrorCodes.VoteTargetNotFound, "...")`.
3. Look up existing vote:
   `IVoteRepository.GetByUserAndTargetAsync(userId, targetType, targetId)`.
4. If no existing vote: create new `Vote` entity. Return `Result.Ok(true)`.
5. If existing vote has same `Voto` as request: idempotent, return `Result.Ok(true)`
   without DB write.
6. If existing vote has different `Voto`: update `vote.Voto = request.Voto`.
   Call `IVoteRepository.UpdateAsync`. Return `Result.Ok(true)`.

### 8.11 `RemoveVoteHandler`

Dependencies: `IVoteRepository`

Accepts: `int userId`, `VoteRequest request`

Note: `request.Voto` is ignored in this handler — any existing vote is removed
regardless of direction.

Logic:
1. Look up vote: `IVoteRepository.GetByUserAndTargetAsync(userId, targetType, targetId)`.
2. If null: return `Result.Ok(true)` (idempotent).
3. Call `IVoteRepository.DeleteAsync(vote)`.
4. Return `Result.Ok(true)`.

---

## SECTION 9 — INFRASTRUCTURE LAYER: IMPLEMENTATIONS

All repository implementations live in
`MusicApp.Infrastructure/Repositories/`. Each injects `AppDbContext`.

### 9.1 `PostRepository`

`GetPagedAsync`:

For `orderBy = "recenti"`:
```csharp
var query = context.Posts
    .Include(p => p.User)
    .OrderByDescending(p => p.CreatedAt);
```

For `orderBy = "votati"`:
```csharp
var query = context.Posts
    .Include(p => p.User)
    .GroupJoin(
        context.Votes.Where(v => v.TargetType == "post"),
        p => p.Id,
        v => v.TargetId,
        (p, votes) => new { Post = p, NetVotes = votes.Count(v => v.Voto == "upvote")
                                                - votes.Count(v => v.Voto == "downvote") })
    .OrderByDescending(x => x.NetVotes)
    .ThenByDescending(x => x.Post.CreatedAt)
    .Select(x => x.Post);
```

Then apply pagination:
```csharp
var total  = await query.CountAsync();
var posts  = await query.Skip((page - 1) * pageSize).Take(pageSize)
                        .AsNoTracking().ToListAsync();
return (posts, total);
```

`GetByIdWithAuthorAsync`:
```csharp
context.Posts.Include(p => p.User)
    .FirstOrDefaultAsync(p => p.Id == id)
```

`SoftDeleteAsync`:
```csharp
post.IsDeleted = true;
post.DeletedAt = DateTime.UtcNow;
post.UpdatedAt = DateTime.UtcNow;
context.Posts.Update(post);
await context.SaveChangesAsync();
```

### 9.2 `CommentRepository`

`GetAllByPostAsync`:
```csharp
context.Comments
    .Include(c => c.User)
    .Where(c => c.PostId == postId)
    .OrderBy(c => c.CreatedAt)
    .AsNoTracking()
    .ToListAsync()
```

`GetTopLevelByPostAsync`:
```csharp
context.Comments
    .Include(c => c.User)
    .Where(c => c.PostId == postId && c.ParentCommentId == null)
    .OrderBy(c => c.CreatedAt)
    .AsNoTracking()
    .ToListAsync()
```

`SoftDeleteAsync`:
```csharp
comment.IsDeleted  = true;
comment.DeletedAt  = DateTime.UtcNow;
comment.UpdatedAt  = DateTime.UtcNow;
context.Comments.Update(comment);
await context.SaveChangesAsync();
```

### 9.3 `VoteRepository`

`GetCountsAsync`:
```csharp
var votes = await context.Votes
    .Where(v => v.TargetType == targetType && v.TargetId == targetId)
    .ToListAsync();
return (votes.Count(v => v.Voto == "upvote"),
        votes.Count(v => v.Voto == "downvote"));
```

`GetCountsBatchAsync`:
```csharp
var ids   = targetIds.ToList();
var votes = await context.Votes
    .Where(v => v.TargetType == targetType && ids.Contains(v.TargetId))
    .GroupBy(v => v.TargetId)
    .Select(g => new {
        TargetId  = g.Key,
        Upvotes   = g.Count(v => v.Voto == "upvote"),
        Downvotes = g.Count(v => v.Voto == "downvote")
    })
    .ToListAsync();

// Build dictionary, ensuring all requested IDs are present (default 0,0)
return ids.ToDictionary(
    id => id,
    id => votes.FirstOrDefault(v => v.TargetId == id) is { } r
          ? (r.Upvotes, r.Downvotes)
          : (0, 0));
```

`GetUserVotesBatchAsync`:
```csharp
var ids   = targetIds.ToList();
var votes = await context.Votes
    .Where(v => v.UserId == userId
             && v.TargetType == targetType
             && ids.Contains(v.TargetId))
    .ToListAsync();

return votes.ToDictionary(v => v.TargetId, v => v.Voto);
```

`DeleteAsync`:
```csharp
context.Votes.Remove(vote);
await context.SaveChangesAsync();
```

---

## SECTION 10 — WEB LAYER: CONTROLLERS

### 10.1 General conventions

Same as previous phases. Additionally in this phase:

**Anonymous endpoints with optional authentication:**
`GET /api/posts`, `GET /api/posts/{id}`, `GET /api/posts/{postId}/comments`
are anonymous but can optionally receive vote status for authenticated users.
In these controllers, extract `userId` as nullable:

```csharp
int? currentUserId = null;
var sub = User.FindFirstValue("sub");
if (sub != null && int.TryParse(sub, out var parsedId))
    currentUserId = parsedId;
```

**OwnerOrAdmin resource check pattern (used in PUT and DELETE endpoints):**
```csharp
// 1. Load resource
var post = await handler.LoadAsync(id);  // or inline load via repository
if (post == null) return NotFound(new ApiError(...));

// 2. Wrap as IOwnedResource
var owned = new OwnedResourceWrapper(post.UserId);  // see Section 10.2

// 3. Authorize
var authResult = await _authorizationService
    .AuthorizeAsync(User, owned, "OwnerOrAdmin");
if (!authResult.Succeeded)
    return StatusCode(403, new ApiError(ErrorCodes.Forbidden,
        "Access denied."));
```

### 10.2 `OwnedResourceWrapper`

File: `MusicApp.Web/Models/OwnedResourceWrapper.cs`

A simple wrapper used by controllers to pass ownership data to the handler:

```csharp
using MusicApp.Application.Authorization;

public class OwnedResourceWrapper : IOwnedResource
{
    public int OwnerId { get; }
    public OwnedResourceWrapper(int ownerId) => OwnerId = ownerId;
}
```

### 10.3 `PostsController`

File: `MusicApp.Web/Controllers/PostsController.cs`
Route prefix: `/api/posts`
Injects: `GetPostsHandler`, `GetPostDetailHandler`, `CreatePostHandler`,
`UpdatePostHandler`, `DeletePostHandler`, `IAuthorizationService`

---

#### `GET /api/posts` — anonymous

Query string bound to `PostListQuery`.

1. Extract optional `currentUserId` (anonymous pattern above).
2. Clamp `PageSize` to max 50 before passing to handler.
3. Call `GetPostsHandler`.
4. Return `200 OK` with `PostListResponse`.

---

#### `GET /api/posts/{id}` — anonymous

1. Extract optional `currentUserId`.
2. Call `GetPostDetailHandler(id, currentUserId)`.
3. On failure: `POST_NOT_FOUND` → 404.
4. Return `200 OK` with `PostDetailDto`.

---

#### `POST /api/posts` — `[Authorize]`

1. Extract `userId`.
2. Call `CreatePostHandler(userId, request)`.
3. Return `201 Created` with `PostSummaryDto`. No Location header required.

---

#### `PUT /api/posts/{id}` — `[Authorize]` + OwnerOrAdmin check

1. Load post via `IPostRepository.GetByIdAsync(id)`.
   If null → 404.
2. Perform OwnerOrAdmin check (see Section 10.1 pattern).
   If fails → 403.
3. If `post.IsDeleted` → return `400 Bad Request` with
   `ApiError(ErrorCodes.PostDeleted, "Cannot edit a deleted post.")`.
4. Call `UpdatePostHandler(id, request)`.
5. On failure: `POST_NOT_FOUND` → 404, `POST_DELETED` → 400.
6. Return `200 OK` with `PostSummaryDto`.

Note: the controller performs the ownership check using the raw entity loaded
in step 1. The handler does NOT re-check ownership — trust is delegated to
the controller's authorization step.

---

#### `DELETE /api/posts/{id}` — `[Authorize]` + OwnerOrAdmin check

1. Load post via `IPostRepository.GetByIdAsync(id)`.
   If null → 404.
2. OwnerOrAdmin check. If fails → 403.
3. If `post.IsDeleted` → return `204 No Content` (idempotent).
4. Call `DeletePostHandler(id)`.
5. Return `204 No Content`.

---

### 10.4 `CommentsController`

File: `MusicApp.Web/Controllers/CommentsController.cs`
Route prefix for `GET /api/posts/{postId}/comments` and
`POST /api/posts/{postId}/comments`: nested under PostsController.
Route prefix for `PUT /api/comments/{id}` and
`DELETE /api/comments/{id}`: `/api/comments`

To handle both route prefixes in a clean way, define a SINGLE controller
`CommentsController` with route-level `[Route]` attributes on individual
actions:

```csharp
[ApiController]
public class CommentsController : ControllerBase
{
    [HttpGet("/api/posts/{postId}/comments")]
    public async Task<IActionResult> GetComments(int postId) { ... }

    [HttpPost("/api/posts/{postId}/comments")]
    [Authorize]
    public async Task<IActionResult> CreateComment(
        int postId, [FromBody] CreateCommentRequest request) { ... }

    [HttpPut("/api/comments/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment(int id, ...) { ... }

    [HttpDelete("/api/comments/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment(int id) { ... }
}
```

Do NOT add a class-level `[Route]` attribute — use explicit paths per action.

---

#### `GET /api/posts/{postId}/comments` — anonymous

1. Extract optional `currentUserId`.
2. Call `GetCommentsHandler(postId, currentUserId)`.
3. Return `200 OK` with `CommentDto[]` (recursive tree).

---

#### `POST /api/posts/{postId}/comments` — `[Authorize]`

1. Extract `userId`.
2. Call `CreateCommentHandler(userId, postId, request)`.
3. On failure:
   - `POST_NOT_FOUND` → 404
   - `POST_DELETED` → 400
   - `COMMENT_NOT_FOUND` → 404 (parent comment not found)
4. Return `201 Created` with `CommentDto`.

---

#### `PUT /api/comments/{id}` — `[Authorize]` + OwnerOrAdmin check

1. Load comment via `ICommentRepository.GetByIdAsync(id)`.
   If null → 404.
2. OwnerOrAdmin check. If fails → 403.
3. If `comment.IsDeleted` → 400 with `COMMENT_DELETED`.
4. Call `UpdateCommentHandler(id, request)`.
5. Return `200 OK` with `CommentDto`.

---

#### `DELETE /api/comments/{id}` — `[Authorize]` + OwnerOrAdmin check

1. Load comment via `ICommentRepository.GetByIdAsync(id)`.
   If null → 404.
2. OwnerOrAdmin check. If fails → 403.
3. If `comment.IsDeleted` → `204 No Content` (idempotent).
4. Call `DeleteCommentHandler(id)`.
5. Return `204 No Content`.

---

### 10.5 `VotesController`

File: `MusicApp.Web/Controllers/VotesController.cs`
Route prefix: `/api/votes`
Both endpoints require `[Authorize]`.

---

#### `POST /api/votes` — `[Authorize]`

1. Extract `userId`.
2. Call `UpsertVoteHandler(userId, request)`.
3. On failure:
   - `VOTE_TARGET_NOT_FOUND` → 404
   - `INVALID_TARGET_TYPE` → 422
4. Return `200 OK` (no body — vote counts are fetched separately).

---

#### `DELETE /api/votes` — `[Authorize]`

Body: `VoteRequest` (only `TargetType` and `TargetId` are used; `Voto`
is present for schema symmetry but ignored).

1. Extract `userId`.
2. Call `RemoveVoteHandler(userId, request)`.
3. Return `204 No Content`.

---

## SECTION 11 — DI REGISTRATION UPDATES

### Program.cs additions (append — do not replace existing registrations)

```csharp
// Phase 3 — OwnerOrAdmin authorization handler
builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrAdminHandler>();
```

Add inside the existing `AddAuthorization` options lambda:
```csharp
options.AddPolicy("OwnerOrAdmin", policy =>
    policy.RequireAuthenticatedUser()
          .RequireClaim("is_active", "True")
          .AddRequirements(new OwnerOrAdminRequirement()));
```

### Infrastructure DI extension additions

Append to `AddInfrastructure`:
```csharp
services.AddScoped<IPostRepository, PostRepository>();
services.AddScoped<ICommentRepository, CommentRepository>();
services.AddScoped<IVoteRepository, VoteRepository>();
```

### Application DI extension additions

Append to `AddApplication`:
```csharp
services.AddScoped<GetPostsHandler>();
services.AddScoped<GetPostDetailHandler>();
services.AddScoped<CreatePostHandler>();
services.AddScoped<UpdatePostHandler>();
services.AddScoped<DeletePostHandler>();
services.AddScoped<GetCommentsHandler>();
services.AddScoped<CreateCommentHandler>();
services.AddScoped<UpdateCommentHandler>();
services.AddScoped<DeleteCommentHandler>();
services.AddScoped<UpsertVoteHandler>();
services.AddScoped<RemoveVoteHandler>();
```

Controllers also need access to `IPostRepository` and `ICommentRepository`
directly for the OwnerOrAdmin pre-load step. Inject them into the relevant
controllers. These repositories are already registered above.

---

## SECTION 12 — VALIDATION CHECKLIST

Before declaring Phase 3 complete, verify every item:

- [ ] `dotnet build` passes with 0 errors across all 4 projects
- [ ] `GET /api/posts` returns 200 without JWT (anonymous works)
- [ ] `GET /api/posts?orderBy=votati` returns posts sorted by net votes
- [ ] `GET /api/posts?page=2&pageSize=5` returns correct slice and TotalCount
- [ ] `GET /api/posts?pageSize=100` returns at most 50 results (clamped)
- [ ] `POST /api/posts` without JWT returns 401
- [ ] `POST /api/posts` with valid JWT creates a post and returns 201
- [ ] `PUT /api/posts/{id}` by the post's author returns 200
- [ ] `PUT /api/posts/{id}` by a different non-Admin user returns 403
- [ ] `PUT /api/posts/{id}` by an Admin user (role = "Admin") returns 200
- [ ] `DELETE /api/posts/{id}` by author sets is_deleted = true (row still exists)
- [ ] `DELETE /api/posts/{id}` called twice returns 204 both times (idempotent)
- [ ] `GET /api/posts/{id}` for a soft-deleted post returns 200 with
      Titolo = "[post rimosso]" and Contenuto = "[contenuto rimosso]"
- [ ] `GET /api/posts/{id}/comments` — commenting is enabled, returns tree
- [ ] Comment tree is correctly nested: child appears in parent's Risposte
- [ ] `POST /api/posts/{postId}/comments` on a deleted post returns 400
- [ ] `POST /api/posts/{postId}/comments` with ParentCommentId from a
      different post returns 404
- [ ] Reply to a soft-deleted comment is allowed and appears in the tree
- [ ] `PUT /api/comments/{id}` on a deleted comment returns 400
- [ ] `DELETE /api/comments/{id}` sets is_deleted = true; child comments
      still appear in the tree with their own content
- [ ] `POST /api/votes` with TargetType = "post", Voto = "upvote" creates a vote
- [ ] `POST /api/votes` called twice with same direction is idempotent (no error,
      no duplicate row in DB)
- [ ] `POST /api/votes` with opposite direction updates the existing vote
- [ ] `DELETE /api/votes` removes the vote row
- [ ] `DELETE /api/votes` when no vote exists returns 204 (idempotent)
- [ ] `GET /api/posts/{id}` for an authenticated user includes UserVote
      field reflecting their current vote
- [ ] `GET /api/posts/{id}` for an anonymous request has UserVote = null
- [ ] `POST /api/votes` on a soft-deleted post returns 404
- [ ] OwnerOrAdmin handler: Admin can edit/delete any post or comment
- [ ] OwnerOrAdmin handler: non-owner non-admin returns 403 on PUT/DELETE

---

## SECTION 13 — OUT OF SCOPE FOR THIS PHASE

Do NOT implement any of the following:

- Notification generation for `commento_risposta` and `post_risposta` —
  the `notifications` table exists from Phase 0 but notification dispatch
  is implemented in Phase 7
- Admin moderation endpoints (`/api/admin/posts/{id}`,
  `/api/admin/comments/{id}`, `moderation_logs`) — Phase 8
- Search or full-text filtering on posts — not planned for this app
- Post/comment reporting system — not planned
- Teacher profiles, slots, bookings (Phase 4)
- Stripe payments (Phase 5)
- Chat or notifications (Phases 6–7)
- Any Razor Page PageModel beyond stubs
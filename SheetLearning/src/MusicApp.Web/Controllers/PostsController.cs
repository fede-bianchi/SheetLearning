using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases.Forum;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/posts")]
[EnableRateLimiting("GeneralPolicy")]
public class PostsController : ControllerBase
{
    private readonly GetPostsHandler _getPostsHandler;
    private readonly GetPostDetailHandler _getPostDetailHandler;
    private readonly CreatePostHandler _createPostHandler;
    private readonly UpdatePostHandler _updatePostHandler;
    private readonly DeletePostHandler _deletePostHandler;
    private readonly IPostRepository _postRepository;
    private readonly IAuthorizationService _authorizationService;

    public PostsController(
        GetPostsHandler getPostsHandler,
        GetPostDetailHandler getPostDetailHandler,
        CreatePostHandler createPostHandler,
        UpdatePostHandler updatePostHandler,
        DeletePostHandler deletePostHandler,
        IPostRepository postRepository,
        IAuthorizationService authorizationService)
    {
        _getPostsHandler = getPostsHandler;
        _getPostDetailHandler = getPostDetailHandler;
        _createPostHandler = createPostHandler;
        _updatePostHandler = updatePostHandler;
        _deletePostHandler = deletePostHandler;
        _postRepository = postRepository;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPosts([FromQuery] PostListQuery query)
    {
        int? currentUserId = null;
        var sub = User.FindFirstValue("sub");
        if (sub != null && int.TryParse(sub, out var parsedId))
        {
            currentUserId = parsedId;
        }

        var safePage = query.Page < 1 ? 1 : query.Page;
        var safePageSize = query.PageSize <= 0 ? 20 : Math.Min(query.PageSize, 50);
        var safeOrderBy = string.IsNullOrWhiteSpace(query.OrderBy) ? "recenti" : query.OrderBy;
        var safeQuery = query with
        {
            Page = safePage,
            PageSize = safePageSize,
            OrderBy = safeOrderBy
        };

        var result = await _getPostsHandler.HandleAsync(safeQuery, currentUserId);
        if (!result.IsSuccess)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."));
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPostById([FromRoute] int id)
    {
        int? currentUserId = null;
        var sub = User.FindFirstValue("sub");
        if (sub != null && int.TryParse(sub, out var parsedId))
        {
            currentUserId = parsedId;
        }

        var result = await _getPostDetailHandler.HandleAsync(id, currentUserId);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.PostNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] CreatePostRequest request)
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _createPostHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."));
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdatePost([FromRoute] int id, [FromBody] UpdatePostRequest request)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post is null)
        {
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.PostNotFound, "Post non trovato."));
        }

        var owned = new OwnedResourceWrapper(post.UserId);
        var authResult = await _authorizationService.AuthorizeAsync(User, owned, "OwnerOrAdmin");
        if (!authResult.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Access denied."));
        }

        if (post.IsDeleted)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(ErrorCodes.PostDeleted, "Cannot edit a deleted post."));
        }

        var result = await _updatePostHandler.HandleAsync(id, request);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.PostNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.PostDeleted => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeletePost([FromRoute] int id)
    {
        var post = await _postRepository.GetByIdAsync(id);
        if (post is null)
        {
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.PostNotFound, "Post non trovato."));
        }

        var owned = new OwnedResourceWrapper(post.UserId);
        var authResult = await _authorizationService.AuthorizeAsync(User, owned, "OwnerOrAdmin");
        if (!authResult.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Access denied."));
        }

        if (post.IsDeleted)
        {
            return NoContent();
        }

        var result = await _deletePostHandler.HandleAsync(id);
        if (!result.IsSuccess && result.ErrorCode == ErrorCodes.PostNotFound)
        {
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(result.ErrorCode, result.ErrorMessage ?? "Post non trovato."));
        }

        return NoContent();
    }
}

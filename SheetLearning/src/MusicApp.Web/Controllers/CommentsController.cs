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
[EnableRateLimiting("GeneralPolicy")]
public class CommentsController : ControllerBase
{
    private readonly GetCommentsHandler _getCommentsHandler;
    private readonly CreateCommentHandler _createCommentHandler;
    private readonly UpdateCommentHandler _updateCommentHandler;
    private readonly DeleteCommentHandler _deleteCommentHandler;
    private readonly ICommentRepository _commentRepository;
    private readonly IAuthorizationService _authorizationService;

    public CommentsController(
        GetCommentsHandler getCommentsHandler,
        CreateCommentHandler createCommentHandler,
        UpdateCommentHandler updateCommentHandler,
        DeleteCommentHandler deleteCommentHandler,
        ICommentRepository commentRepository,
        IAuthorizationService authorizationService)
    {
        _getCommentsHandler = getCommentsHandler;
        _createCommentHandler = createCommentHandler;
        _updateCommentHandler = updateCommentHandler;
        _deleteCommentHandler = deleteCommentHandler;
        _commentRepository = commentRepository;
        _authorizationService = authorizationService;
    }

    [HttpGet("/api/posts/{postId:int}/comments")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComments([FromRoute] int postId)
    {
        int? currentUserId = null;
        var sub = User.FindFirstValue("sub");
        if (sub != null && int.TryParse(sub, out var parsedId))
        {
            currentUserId = parsedId;
        }

        var result = await _getCommentsHandler.HandleAsync(postId, currentUserId);
        if (!result.IsSuccess)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."));
        }

        return Ok(result.Value);
    }

    [HttpPost("/api/posts/{postId:int}/comments")]
    [Authorize]
    public async Task<IActionResult> CreateComment([FromRoute] int postId, [FromBody] CreateCommentRequest request)
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _createCommentHandler.HandleAsync(userId, postId, request);

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
                ErrorCodes.CommentNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("/api/comments/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateComment([FromRoute] int id, [FromBody] UpdateCommentRequest request)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment is null)
        {
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.CommentNotFound, "Commento non trovato."));
        }

        var owned = new OwnedResourceWrapper(comment.UserId);
        var authResult = await _authorizationService.AuthorizeAsync(User, owned, "OwnerOrAdmin");
        if (!authResult.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Access denied."));
        }

        if (comment.IsDeleted)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(ErrorCodes.CommentDeleted, "Cannot edit a deleted comment."));
        }

        var result = await _updateCommentHandler.HandleAsync(id, request);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.CommentNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.CommentDeleted => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpDelete("/api/comments/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment([FromRoute] int id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment is null)
        {
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.CommentNotFound, "Commento non trovato."));
        }

        var owned = new OwnedResourceWrapper(comment.UserId);
        var authResult = await _authorizationService.AuthorizeAsync(User, owned, "OwnerOrAdmin");
        if (!authResult.Succeeded)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Access denied."));
        }

        if (comment.IsDeleted)
        {
            return NoContent();
        }

        var result = await _deleteCommentHandler.HandleAsync(id);
        if (!result.IsSuccess && result.ErrorCode == ErrorCodes.CommentNotFound)
        {
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(result.ErrorCode, result.ErrorMessage ?? "Commento non trovato."));
        }

        return NoContent();
    }
}

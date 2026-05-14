using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Admin;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Authorize(Policy = "IsAdmin")]
public class AdminContentController : ControllerBase
{
    private readonly AdminDeletePostHandler _adminDeletePostHandler;
    private readonly AdminDeleteCommentHandler _adminDeleteCommentHandler;
    private readonly GetModerationLogsHandler _getModerationLogsHandler;

    public AdminContentController(
        AdminDeletePostHandler adminDeletePostHandler,
        AdminDeleteCommentHandler adminDeleteCommentHandler,
        GetModerationLogsHandler getModerationLogsHandler)
    {
        _adminDeletePostHandler = adminDeletePostHandler;
        _adminDeleteCommentHandler = adminDeleteCommentHandler;
        _getModerationLogsHandler = getModerationLogsHandler;
    }

    [HttpDelete("/api/admin/posts/{id:int}")]
    public async Task<IActionResult> DeletePost([FromRoute] int id, [FromBody] ModerationActionRequest request)
    {
        var adminId = GetAdminId();
        var result = await _adminDeletePostHandler.HandleAsync(adminId, id, request.Motivazione!);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.PostNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }

    [HttpDelete("/api/admin/comments/{id:int}")]
    public async Task<IActionResult> DeleteComment([FromRoute] int id, [FromBody] ModerationActionRequest request)
    {
        var adminId = GetAdminId();
        var result = await _adminDeleteCommentHandler.HandleAsync(adminId, id, request.Motivazione!);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.CommentNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }

    [HttpGet("/api/admin/moderation-logs")]
    public async Task<IActionResult> GetModerationLogs([FromQuery] ModerationLogQuery query)
    {
        var result = await _getModerationLogsHandler.HandleAsync(query);
        return Ok(result.Value);
    }

    private int GetAdminId() => int.Parse(User.FindFirstValue("sub")!);
}

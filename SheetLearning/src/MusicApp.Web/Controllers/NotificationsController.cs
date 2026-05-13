using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Notifications;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly GetMyNotificationsHandler _getMyNotificationsHandler;
    private readonly MarkNotificationReadHandler _markNotificationReadHandler;
    private readonly MarkAllNotificationsReadHandler _markAllNotificationsReadHandler;
    private readonly ArchiveNotificationHandler _archiveNotificationHandler;
    private readonly GetUnreadCountHandler _getUnreadCountHandler;

    public NotificationsController(
        GetMyNotificationsHandler getMyNotificationsHandler,
        MarkNotificationReadHandler markNotificationReadHandler,
        MarkAllNotificationsReadHandler markAllNotificationsReadHandler,
        ArchiveNotificationHandler archiveNotificationHandler,
        GetUnreadCountHandler getUnreadCountHandler)
    {
        _getMyNotificationsHandler = getMyNotificationsHandler;
        _markNotificationReadHandler = markNotificationReadHandler;
        _markAllNotificationsReadHandler = markAllNotificationsReadHandler;
        _archiveNotificationHandler = archiveNotificationHandler;
        _getUnreadCountHandler = getUnreadCountHandler;
    }

    [HttpGet("unread-count")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var result = await _getUnreadCountHandler.HandleAsync(userId);
        return Ok(result.Value);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationQuery query)
    {
        var userId = GetCurrentUserId();
        var pageSize = Math.Min(query.PageSize, 50);
        var clampedQuery = new NotificationQuery(query.Page, pageSize);
        var result = await _getMyNotificationsHandler.HandleAsync(userId, clampedQuery);
        return Ok(result.Value);
    }

    [HttpPost("{id:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkRead([FromRoute] int id)
    {
        var userId = GetCurrentUserId();
        var result = await _markNotificationReadHandler.HandleAsync(userId, id);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.NotificationNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return NoContent();
    }

    [HttpPost("read-all")]
    [Authorize]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = GetCurrentUserId();
        await _markAllNotificationsReadHandler.HandleAsync(userId);
        return NoContent();
    }

    [HttpPost("{id:int}/archive")]
    [Authorize]
    public async Task<IActionResult> Archive([FromRoute] int id)
    {
        var userId = GetCurrentUserId();
        var result = await _archiveNotificationHandler.HandleAsync(userId, id);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.NotificationNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue("sub")!);
    }
}

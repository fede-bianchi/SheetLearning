using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases.Chat;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly GetMyChatsHandler _getMyChatsHandler;
    private readonly GetChatMessagesHandler _getChatMessagesHandler;
    private readonly SendMessageHandler _sendMessageHandler;
    private readonly MarkMessagesReadHandler _markMessagesReadHandler;
    private readonly IChatRepository _chatRepo;

    public ChatsController(
        GetMyChatsHandler getMyChatsHandler,
        GetChatMessagesHandler getChatMessagesHandler,
        SendMessageHandler sendMessageHandler,
        MarkMessagesReadHandler markMessagesReadHandler,
        IChatRepository chatRepo)
    {
        _getMyChatsHandler = getMyChatsHandler;
        _getChatMessagesHandler = getChatMessagesHandler;
        _sendMessageHandler = sendMessageHandler;
        _markMessagesReadHandler = markMessagesReadHandler;
        _chatRepo = chatRepo;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyChats()
    {
        var userId = GetCurrentUserId();
        var result = await _getMyChatsHandler.HandleAsync(userId);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}/messages")]
    [Authorize]
    public async Task<IActionResult> GetChatMessages(
        [FromRoute] int id, [FromQuery] MessageQuery query)
    {
        var userId = GetCurrentUserId();
        var chat = await _chatRepo.GetByIdAsync(id);
        if (chat == null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.ChatNotFound, "Chat non trovata."));

        if (chat.StudentId != userId
            && chat.TeacherId != userId
            && !User.HasClaim("role", "Admin"))
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _getChatMessagesHandler.HandleAsync(
            id, query.Page, query.PageSize);
        return Ok(result.Value);
    }

    [HttpPost("{teacherId:int}/messages")]
    [Authorize]
    public async Task<IActionResult> SendMessage(
        [FromRoute] int teacherId, [FromBody] SendMessageRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _sendMessageHandler.HandleAsync(
            userId, teacherId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.CannotMessageSelf => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.TeacherNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("{id:int}/read")]
    [Authorize]
    public async Task<IActionResult> MarkRead([FromRoute] int id)
    {
        var userId = GetCurrentUserId();
        var chat = await _chatRepo.GetByIdAsync(id);
        if (chat == null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.ChatNotFound, "Chat non trovata."));

        if (chat.StudentId != userId
            && chat.TeacherId != userId
            && !User.HasClaim("role", "Admin"))
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _markMessagesReadHandler.HandleAsync(id, userId);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.ChatNotFound => StatusCode(
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

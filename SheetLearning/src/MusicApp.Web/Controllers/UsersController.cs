using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases.Exercises;
using MusicApp.Application.UseCases.Users;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly GetMyProfileHandler _getMyProfileHandler;
    private readonly UpdateMyProfileHandler _updateMyProfileHandler;
    private readonly ChangePasswordHandler _changePasswordHandler;
    private readonly DeleteMyAccountHandler _deleteMyAccountHandler;
    private readonly GetPublicProfileHandler _getPublicProfileHandler;
    private readonly GetMyProgressHandler _getMyProgressHandler;
    private readonly GetMyBestScoresHandler _getMyBestScoresHandler;
    private readonly GetMyAttemptsHandler _getMyAttemptsHandler;
    private readonly IRefreshCookieHelper _refreshCookieHelper;

    public UsersController(
        GetMyProfileHandler getMyProfileHandler,
        UpdateMyProfileHandler updateMyProfileHandler,
        ChangePasswordHandler changePasswordHandler,
        DeleteMyAccountHandler deleteMyAccountHandler,
        GetPublicProfileHandler getPublicProfileHandler,
        GetMyProgressHandler getMyProgressHandler,
        GetMyBestScoresHandler getMyBestScoresHandler,
        GetMyAttemptsHandler getMyAttemptsHandler,
        IRefreshCookieHelper refreshCookieHelper)
    {
        _getMyProfileHandler = getMyProfileHandler;
        _updateMyProfileHandler = updateMyProfileHandler;
        _changePasswordHandler = changePasswordHandler;
        _deleteMyAccountHandler = deleteMyAccountHandler;
        _getPublicProfileHandler = getPublicProfileHandler;
        _getMyProgressHandler = getMyProgressHandler;
        _getMyBestScoresHandler = getMyBestScoresHandler;
        _getMyAttemptsHandler = getMyAttemptsHandler;
        _refreshCookieHelper = refreshCookieHelper;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _getMyProfileHandler.HandleAsync(userId);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.UserNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpPatch("me")]
    [Authorize]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _updateMyProfileHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.NicknameConflict => StatusCode(
                    StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpPatch("me/password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _changePasswordHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.WrongPassword => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return NoContent();
    }

    [HttpDelete("me")]
    [Authorize(Policy = "CanDeleteOwnAccount")]
    public async Task<IActionResult> DeleteMyAccount([FromBody] DeleteAccountRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _deleteMyAccountHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.WrongPassword => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        _refreshCookieHelper.ClearRefreshCookie(Response);
        return NoContent();
    }

    [HttpGet("{id:int}/public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicProfile([FromRoute] int id)
    {
        var result = await _getPublicProfileHandler.HandleAsync(id);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.UserNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpGet("me/progress")]
    [Authorize]
    public async Task<IActionResult> GetMyProgress()
    {
        var userId = GetCurrentUserId();
        var result = await _getMyProgressHandler.HandleAsync(userId);

        if (!result.IsSuccess)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."));
        }

        return Ok(result.Value);
    }

    [HttpGet("me/best-scores")]
    [Authorize]
    public async Task<IActionResult> GetMyBestScores()
    {
        var userId = GetCurrentUserId();
        var result = await _getMyBestScoresHandler.HandleAsync(userId);

        if (!result.IsSuccess)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."));
        }

        return Ok(result.Value);
    }

    [HttpGet("me/attempts")]
    [Authorize]
    public async Task<IActionResult> GetMyAttempts([FromQuery] AttemptQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _getMyAttemptsHandler.HandleAsync(userId, query);

        if (!result.IsSuccess)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."));
        }

        return Ok(result.Value);
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue("sub")!);
    }
}

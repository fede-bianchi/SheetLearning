using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases.Auth;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerUserHandler;
    private readonly LoginUserHandler _loginUserHandler;
    private readonly RefreshTokenHandler _refreshTokenHandler;
    private readonly LogoutHandler _logoutHandler;
    private readonly LogoutAllHandler _logoutAllHandler;
    private readonly IRefreshCookieHelper _refreshCookieHelper;

    public AuthController(
        RegisterUserHandler registerUserHandler,
        LoginUserHandler loginUserHandler,
        RefreshTokenHandler refreshTokenHandler,
        LogoutHandler logoutHandler,
        LogoutAllHandler logoutAllHandler,
        IRefreshCookieHelper refreshCookieHelper)
    {
        _registerUserHandler = registerUserHandler;
        _loginUserHandler = loginUserHandler;
        _refreshTokenHandler = refreshTokenHandler;
        _logoutHandler = logoutHandler;
        _logoutAllHandler = logoutAllHandler;
        _refreshCookieHelper = refreshCookieHelper;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _registerUserHandler.HandleAsync(request);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.EmailAlreadyExists => StatusCode(
                    StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.NicknameAlreadyExists => StatusCode(
                    StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UnderAge => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        var payload = result.Value!;
        _refreshCookieHelper.SetRefreshCookie(Response, payload.RefreshToken, payload.RefreshTokenExpiry);
        return Ok(payload.Response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _loginUserHandler.HandleAsync(request);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.InvalidCredentials => StatusCode(
                    StatusCodes.Status401Unauthorized,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.AccountInactive => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        var payload = result.Value!;
        _refreshCookieHelper.SetRefreshCookie(Response, payload.RefreshToken, payload.RefreshTokenExpiry);
        return Ok(payload.Response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        var token = _refreshCookieHelper.ReadRefreshToken(Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return StatusCode(
                StatusCodes.Status401Unauthorized,
                new ApiError(ErrorCodes.InvalidRefreshToken, "Refresh token non valido."));
        }

        var result = await _refreshTokenHandler.HandleAsync(token);
        if (!result.IsSuccess)
        {
            if (result.ErrorCode is ErrorCodes.InvalidRefreshToken or ErrorCodes.ExpiredRefreshToken or ErrorCodes.AccountInactive)
            {
                _refreshCookieHelper.ClearRefreshCookie(Response);
            }

            return result.ErrorCode switch
            {
                ErrorCodes.InvalidRefreshToken => StatusCode(
                    StatusCodes.Status401Unauthorized,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.ExpiredRefreshToken => StatusCode(
                    StatusCodes.Status401Unauthorized,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.AccountInactive => StatusCode(
                    StatusCodes.Status403Forbidden,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        var payload = result.Value!;
        _refreshCookieHelper.SetRefreshCookie(Response, payload.RefreshToken, payload.RefreshTokenExpiry);
        return Ok(payload.Response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var token = _refreshCookieHelper.ReadRefreshToken(Request);
        if (string.IsNullOrWhiteSpace(token))
        {
            return NoContent();
        }

        await _logoutHandler.HandleAsync(token);
        _refreshCookieHelper.ClearRefreshCookie(Response);
        return NoContent();
    }

    [HttpPost("logout-all")]
    [Authorize]
    public async Task<IActionResult> LogoutAll()
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);

        await _logoutAllHandler.HandleAsync(userId);
        _refreshCookieHelper.ClearRefreshCookie(Response);
        return NoContent();
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Admin;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "IsAdmin")]
public class AdminUsersController : ControllerBase
{
    private readonly GetAdminUsersHandler _getAdminUsersHandler;
    private readonly GetAdminUserDetailHandler _getAdminUserDetailHandler;
    private readonly BanUserHandler _banUserHandler;
    private readonly UnbanUserHandler _unbanUserHandler;
    private readonly WarnUserHandler _warnUserHandler;
    private readonly ChangeUserRoleHandler _changeUserRoleHandler;
    private readonly DeleteAdminUserHandler _deleteAdminUserHandler;

    public AdminUsersController(
        GetAdminUsersHandler getAdminUsersHandler,
        GetAdminUserDetailHandler getAdminUserDetailHandler,
        BanUserHandler banUserHandler,
        UnbanUserHandler unbanUserHandler,
        WarnUserHandler warnUserHandler,
        ChangeUserRoleHandler changeUserRoleHandler,
        DeleteAdminUserHandler deleteAdminUserHandler)
    {
        _getAdminUsersHandler = getAdminUsersHandler;
        _getAdminUserDetailHandler = getAdminUserDetailHandler;
        _banUserHandler = banUserHandler;
        _unbanUserHandler = unbanUserHandler;
        _warnUserHandler = warnUserHandler;
        _changeUserRoleHandler = changeUserRoleHandler;
        _deleteAdminUserHandler = deleteAdminUserHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] AdminUserQuery query)
    {
        var result = await _getAdminUsersHandler.HandleAsync(query);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUser([FromRoute] int id)
    {
        var result = await _getAdminUserDetailHandler.HandleAsync(id);
        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.UserNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}/ban")]
    public async Task<IActionResult> BanUser([FromRoute] int id, [FromBody] ModerationActionRequest request)
    {
        var adminId = GetAdminId();
        var result = await _banUserHandler.HandleAsync(adminId, id, request.Motivazione!);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.AdminCannotTargetSelf => StatusCode(StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserAlreadyBanned => StatusCode(StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }

    [HttpPatch("{id:int}/unban")]
    public async Task<IActionResult> UnbanUser([FromRoute] int id)
    {
        var adminId = GetAdminId();
        var result = await _unbanUserHandler.HandleAsync(adminId, id);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.UserNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserNotBanned => StatusCode(StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }

    [HttpPatch("{id:int}/warn")]
    public async Task<IActionResult> WarnUser([FromRoute] int id, [FromBody] ModerationActionRequest request)
    {
        var adminId = GetAdminId();
        var result = await _warnUserHandler.HandleAsync(adminId, id, request.Motivazione!);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.UserNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }

    [HttpPatch("{id:int}/role")]
    public async Task<IActionResult> ChangeRole([FromRoute] int id, [FromBody] ChangeRoleRequest request)
    {
        var adminId = GetAdminId();
        var result = await _changeUserRoleHandler.HandleAsync(adminId, id, request.NuovoRuolo);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.AdminCannotChangeOwnRole => StatusCode(StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.InvalidRole => StatusCode(StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id, [FromBody] ModerationActionRequest request)
    {
        var adminId = GetAdminId();
        var result = await _deleteAdminUserHandler.HandleAsync(adminId, id, request.Motivazione!);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.AdminCannotTargetSelf => StatusCode(StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.UserNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }

    private int GetAdminId() => int.Parse(User.FindFirstValue("sub")!);
}

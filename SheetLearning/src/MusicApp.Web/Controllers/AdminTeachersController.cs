using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.UseCases.Admin;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/admin/teachers")]
[Authorize(Policy = "IsAdmin")]
public class AdminTeachersController : ControllerBase
{
    private readonly GetAdminTeachersHandler _getAdminTeachersHandler;
    private readonly GetTeacherRatingStatsHandler _getTeacherRatingStatsHandler;

    public AdminTeachersController(
        GetAdminTeachersHandler getAdminTeachersHandler,
        GetTeacherRatingStatsHandler getTeacherRatingStatsHandler)
    {
        _getAdminTeachersHandler = getAdminTeachersHandler;
        _getTeacherRatingStatsHandler = getTeacherRatingStatsHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetTeachers()
    {
        var result = await _getAdminTeachersHandler.HandleAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id:int}/ratings")]
    public async Task<IActionResult> GetRatings([FromRoute] int id)
    {
        var result = await _getTeacherRatingStatsHandler.HandleAsync(id);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.TeacherProfileNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return Ok(result.Value);
    }
}

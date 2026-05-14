using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Exercises;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("GeneralPolicy")]
public class AttemptsController : ControllerBase
{
    private readonly SaveAttemptHandler _saveAttemptHandler;

    public AttemptsController(SaveAttemptHandler saveAttemptHandler)
    {
        _saveAttemptHandler = saveAttemptHandler;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SaveAttempt([FromBody] CreateAttemptRequest request)
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _saveAttemptHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.ExerciseTypeNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.LevelNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.LevelExerciseMismatch => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.InvalidDifficolta => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.InvalidPunteggio => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}

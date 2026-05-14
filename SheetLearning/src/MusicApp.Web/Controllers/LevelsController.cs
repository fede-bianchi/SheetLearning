using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using MusicApp.Application.Common;
using MusicApp.Application.UseCases.Exercises;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[EnableRateLimiting("GeneralPolicy")]
public class LevelsController : ControllerBase
{
    private readonly GetLevelsHandler _getLevelsHandler;
    private readonly GetLevelDetailHandler _getLevelDetailHandler;
    private readonly GetLevelsByExerciseTypeHandler _getLevelsByExerciseTypeHandler;

    public LevelsController(
        GetLevelsHandler getLevelsHandler,
        GetLevelDetailHandler getLevelDetailHandler,
        GetLevelsByExerciseTypeHandler getLevelsByExerciseTypeHandler)
    {
        _getLevelsHandler = getLevelsHandler;
        _getLevelDetailHandler = getLevelDetailHandler;
        _getLevelsByExerciseTypeHandler = getLevelsByExerciseTypeHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetLevels()
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _getLevelsHandler.HandleAsync(userId);
        return Ok(result.Value);
    }

    [HttpGet("exercise/{exerciseTypeId:int}")]
    public async Task<IActionResult> GetLevelsByExerciseType([FromRoute] int exerciseTypeId)
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _getLevelsByExerciseTypeHandler.HandleAsync(userId, exerciseTypeId);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.ExerciseTypeNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetLevelDetail([FromRoute] int id)
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _getLevelDetailHandler.HandleAsync(userId, id);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.LevelNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }
}

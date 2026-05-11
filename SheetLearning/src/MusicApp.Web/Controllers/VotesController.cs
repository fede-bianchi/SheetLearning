using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Forum;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/votes")]
[Authorize]
public class VotesController : ControllerBase
{
    private readonly UpsertVoteHandler _upsertVoteHandler;
    private readonly RemoveVoteHandler _removeVoteHandler;

    public VotesController(
        UpsertVoteHandler upsertVoteHandler,
        RemoveVoteHandler removeVoteHandler)
    {
        _upsertVoteHandler = upsertVoteHandler;
        _removeVoteHandler = removeVoteHandler;
    }

    [HttpPost]
    public async Task<IActionResult> UpsertVote([FromBody] VoteRequest request)
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _upsertVoteHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.VoteTargetNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.InvalidTargetType => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveVote([FromBody] VoteRequest request)
    {
        var userId = int.Parse(User.FindFirstValue("sub")!);
        await _removeVoteHandler.HandleAsync(userId, request);
        return NoContent();
    }
}

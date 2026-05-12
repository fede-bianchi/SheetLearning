using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.UseCases.Payments;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubscriptionsController : ControllerBase
{
    private readonly CancelSubscriptionHandler _cancelSubscriptionHandler;

    public SubscriptionsController(
        CancelSubscriptionHandler cancelSubscriptionHandler)
    {
        _cancelSubscriptionHandler = cancelSubscriptionHandler;
    }

    [HttpPost("cancel")]
    [Authorize]
    public async Task<IActionResult> CancelSubscription()
    {
        var userId = GetCurrentUserId();
        var result = await _cancelSubscriptionHandler.HandleAsync(userId);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.NoActiveSubscription => StatusCode(
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

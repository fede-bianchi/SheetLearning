using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Admin;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Policy = "IsAdmin")]
public class AdminStatsController : ControllerBase
{
    private readonly GetPlatformOverviewHandler _getPlatformOverviewHandler;
    private readonly GetRevenueStatsHandler _getRevenueStatsHandler;

    public AdminStatsController(
        GetPlatformOverviewHandler getPlatformOverviewHandler,
        GetRevenueStatsHandler getRevenueStatsHandler)
    {
        _getPlatformOverviewHandler = getPlatformOverviewHandler;
        _getRevenueStatsHandler = getRevenueStatsHandler;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        var result = await _getPlatformOverviewHandler.HandleAsync();
        return Ok(result.Value);
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] DateRangeQuery query)
    {
        var result = await _getRevenueStatsHandler.HandleAsync(query);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.InvalidDateRange => StatusCode(StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return Ok(result.Value);
    }
}

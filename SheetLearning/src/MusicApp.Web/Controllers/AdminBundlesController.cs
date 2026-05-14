using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Admin;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/admin/lesson-bundles")]
[Authorize(Policy = "IsAdmin")]
public class AdminBundlesController : ControllerBase
{
    private readonly GetAdminBundlesHandler _getAdminBundlesHandler;
    private readonly CreateBundleHandler _createBundleHandler;
    private readonly UpdateBundleHandler _updateBundleHandler;
    private readonly ToggleBundleHandler _toggleBundleHandler;

    public AdminBundlesController(
        GetAdminBundlesHandler getAdminBundlesHandler,
        CreateBundleHandler createBundleHandler,
        UpdateBundleHandler updateBundleHandler,
        ToggleBundleHandler toggleBundleHandler)
    {
        _getAdminBundlesHandler = getAdminBundlesHandler;
        _createBundleHandler = createBundleHandler;
        _updateBundleHandler = updateBundleHandler;
        _toggleBundleHandler = toggleBundleHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetBundles()
    {
        var result = await _getAdminBundlesHandler.HandleAsync();
        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBundle([FromBody] CreateBundleRequest request)
    {
        var result = await _createBundleHandler.HandleAsync(request);
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateBundle([FromRoute] int id, [FromBody] UpdateBundleRequest request)
    {
        var result = await _updateBundleHandler.HandleAsync(id, request);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.BundleNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> ToggleBundle([FromRoute] int id)
    {
        var result = await _toggleBundleHandler.HandleAsync(id);

        if (!result.IsSuccess)
            return result.ErrorCode switch
            {
                ErrorCodes.BundleNotFound => StatusCode(StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST", result.ErrorMessage ?? "Errore."))
            };

        return NoContent();
    }
}

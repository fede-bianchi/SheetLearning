using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.UseCases.Bundles;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/lesson-bundles")]
public class LessonBundlesController : ControllerBase
{
    private readonly GetActiveBundlesHandler _getActiveBundlesHandler;

    public LessonBundlesController(GetActiveBundlesHandler getActiveBundlesHandler)
    {
        _getActiveBundlesHandler = getActiveBundlesHandler;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveBundles()
    {
        var result = await _getActiveBundlesHandler.HandleAsync();
        return Ok(result.Value);
    }
}

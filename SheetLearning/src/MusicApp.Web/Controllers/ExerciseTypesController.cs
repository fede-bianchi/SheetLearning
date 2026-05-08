using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.UseCases.Exercises;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/exercise-types")]
public class ExerciseTypesController : ControllerBase
{
    private readonly GetExerciseTypesHandler _getExerciseTypesHandler;

    public ExerciseTypesController(GetExerciseTypesHandler getExerciseTypesHandler)
    {
        _getExerciseTypesHandler = getExerciseTypesHandler;
    }

    [HttpGet]
    public async Task<IActionResult> GetExerciseTypes()
    {
        var result = await _getExerciseTypesHandler.HandleAsync();
        return Ok(result.Value);
    }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Authorization;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases.Slots;
using MusicApp.Application.UseCases.Teachers;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeachersController : ControllerBase
{
    private readonly GetTeachersHandler _getTeachersHandler;
    private readonly GetTeacherPublicProfileHandler _getTeacherPublicProfileHandler;
    private readonly GetMyTeacherProfileHandler _getMyTeacherProfileHandler;
    private readonly UpdateMyTeacherProfileHandler _updateMyTeacherProfileHandler;
    private readonly SetMyCategoriesHandler _setMyCategoriesHandler;
    private readonly GetTeacherSlotsHandler _getTeacherSlotsHandler;
    private readonly GetMyTeacherSlotsHandler _getMyTeacherSlotsHandler;
    private readonly CreateSlotHandler _createSlotHandler;
    private readonly DeleteSlotHandler _deleteSlotHandler;
    private readonly GetMyStudentsHandler _getMyStudentsHandler;
    private readonly GetStudentDetailHandler _getStudentDetailHandler;
    private readonly GetStudentBookingsHandler _getStudentBookingsHandler;
    private readonly ILessonSlotRepository _slotRepository;
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly IAuthorizationService _authorizationService;

    public TeachersController(
        GetTeachersHandler getTeachersHandler,
        GetTeacherPublicProfileHandler getTeacherPublicProfileHandler,
        GetMyTeacherProfileHandler getMyTeacherProfileHandler,
        UpdateMyTeacherProfileHandler updateMyTeacherProfileHandler,
        SetMyCategoriesHandler setMyCategoriesHandler,
        GetTeacherSlotsHandler getTeacherSlotsHandler,
        GetMyTeacherSlotsHandler getMyTeacherSlotsHandler,
        CreateSlotHandler createSlotHandler,
        DeleteSlotHandler deleteSlotHandler,
        GetMyStudentsHandler getMyStudentsHandler,
        GetStudentDetailHandler getStudentDetailHandler,
        GetStudentBookingsHandler getStudentBookingsHandler,
        ILessonSlotRepository slotRepository,
        ILessonBookingRepository bookingRepository,
        IAuthorizationService authorizationService)
    {
        _getTeachersHandler = getTeachersHandler;
        _getTeacherPublicProfileHandler = getTeacherPublicProfileHandler;
        _getMyTeacherProfileHandler = getMyTeacherProfileHandler;
        _updateMyTeacherProfileHandler = updateMyTeacherProfileHandler;
        _setMyCategoriesHandler = setMyCategoriesHandler;
        _getTeacherSlotsHandler = getTeacherSlotsHandler;
        _getMyTeacherSlotsHandler = getMyTeacherSlotsHandler;
        _createSlotHandler = createSlotHandler;
        _deleteSlotHandler = deleteSlotHandler;
        _getMyStudentsHandler = getMyStudentsHandler;
        _getStudentDetailHandler = getStudentDetailHandler;
        _getStudentBookingsHandler = getStudentBookingsHandler;
        _slotRepository = slotRepository;
        _bookingRepository = bookingRepository;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeachers([FromQuery] TeacherListQuery query)
    {
        var planClaim = User.FindFirstValue("plan");
        var roleClaim = User.FindFirstValue("role");

        var result = await _getTeachersHandler.HandleAsync(query, planClaim, roleClaim);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTeacherPublicProfile([FromRoute] int id)
    {
        var planClaim = User.FindFirstValue("plan");
        var roleClaim = User.FindFirstValue("role");

        var result = await _getTeacherPublicProfileHandler.HandleAsync(
            id, planClaim, roleClaim);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.TeacherProfileNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.TeacherNotVisible => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpGet("me")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> GetMyTeacherProfile()
    {
        var userId = GetCurrentUserId();
        var result = await _getMyTeacherProfileHandler.HandleAsync(userId);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.TeacherProfileNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpPut("me")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> UpdateMyTeacherProfile(
        [FromBody] UpdateTeacherProfileRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _updateMyTeacherProfileHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.TeacherProfileNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpPut("me/categories")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> SetMyCategories(
        [FromBody] SetCategoriesRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _setMyCategoriesHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.TeacherProfileNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.InvalidCategoryCount => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return NoContent();
    }

    [HttpGet("{id:int}/slots")]
    [Authorize]
    public async Task<IActionResult> GetTeacherSlots(
        [FromRoute] int id, [FromQuery] SlotQuery query)
    {
        var result = await _getTeacherSlotsHandler.HandleAsync(id, query);
        return Ok(result.Value);
    }

    [HttpGet("me/slots")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> GetMyTeacherSlots([FromQuery] SlotQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _getMyTeacherSlotsHandler.HandleAsync(userId, query);
        return Ok(result.Value);
    }

    [HttpPost("me/slots")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _createSlotHandler.HandleAsync(userId, request);
        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpDelete("me/slots/{slotId:int}")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> DeleteSlot([FromRoute] int slotId)
    {
        var userId = GetCurrentUserId();
        var slot = await _slotRepository.GetByIdAsync(slotId);
        if (slot is null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.SlotNotFound, "Slot non trovato."));

        var authResult = await _authorizationService.AuthorizeAsync(
            User, new OwnedResourceWrapper(slot.TeacherId), "TeacherOwnsSlot");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _deleteSlotHandler.HandleAsync(slotId);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.SlotNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.SlotAlreadyBooked => StatusCode(
                    StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return NoContent();
    }

    [HttpGet("me/students")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> GetMyStudents()
    {
        var userId = GetCurrentUserId();
        var result = await _getMyStudentsHandler.HandleAsync(userId);
        return Ok(result.Value);
    }

    [HttpGet("me/students/{studentId:int}")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> GetStudentDetail([FromRoute] int studentId)
    {
        var userId = GetCurrentUserId();

        var resource = new TeacherStudentResource
            { TeacherId = userId, StudentId = studentId };
        var authResult = await _authorizationService.AuthorizeAsync(
            User, resource, "TeacherManagesStudent");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _getStudentDetailHandler.HandleAsync(userId, studentId);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.StudentNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpGet("me/students/{studentId:int}/bookings")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> GetStudentBookings([FromRoute] int studentId)
    {
        var userId = GetCurrentUserId();

        var resource = new TeacherStudentResource
            { TeacherId = userId, StudentId = studentId };
        var authResult = await _authorizationService.AuthorizeAsync(
            User, resource, "TeacherManagesStudent");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _getStudentBookingsHandler.HandleAsync(
            userId, studentId);
        return Ok(result.Value);
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue("sub")!);
    }
}

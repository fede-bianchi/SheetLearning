using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Authorization;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases.Bookings;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly GetMyBookingsHandler _getMyBookingsHandler;
    private readonly GetBookingDetailHandler _getBookingDetailHandler;
    private readonly CreateBookingHandler _createBookingHandler;
    private readonly ConfirmBookingHandler _confirmBookingHandler;
    private readonly CancelBookingHandler _cancelBookingHandler;
    private readonly CompleteBookingHandler _completeBookingHandler;
    private readonly CreateRatingHandler _createRatingHandler;
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly IAuthorizationService _authorizationService;

    public BookingsController(
        GetMyBookingsHandler getMyBookingsHandler,
        GetBookingDetailHandler getBookingDetailHandler,
        CreateBookingHandler createBookingHandler,
        ConfirmBookingHandler confirmBookingHandler,
        CancelBookingHandler cancelBookingHandler,
        CompleteBookingHandler completeBookingHandler,
        CreateRatingHandler createRatingHandler,
        ILessonBookingRepository bookingRepository,
        IAuthorizationService authorizationService)
    {
        _getMyBookingsHandler = getMyBookingsHandler;
        _getBookingDetailHandler = getBookingDetailHandler;
        _createBookingHandler = createBookingHandler;
        _confirmBookingHandler = confirmBookingHandler;
        _cancelBookingHandler = cancelBookingHandler;
        _completeBookingHandler = completeBookingHandler;
        _createRatingHandler = createRatingHandler;
        _bookingRepository = bookingRepository;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetMyBookings([FromQuery] BookingListQuery query)
    {
        var userId = GetCurrentUserId();
        var result = await _getMyBookingsHandler.HandleAsync(userId, query);
        return Ok(result.Value);
    }

    [HttpGet("{id:int}")]
    [Authorize]
    public async Task<IActionResult> GetBookingDetail([FromRoute] int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.BookingNotFound, "Prenotazione non trovata."));

        var resource = new BookingParticipantResource
            { TeacherId = booking.TeacherId, StudentId = booking.StudentId };
        var authResult = await _authorizationService.AuthorizeAsync(
            User, resource, "BookingParticipant");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _getBookingDetailHandler.HandleAsync(id);
        return Ok(result.Value);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _createBookingHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.SlotNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.SlotNotAvailable => StatusCode(
                    StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.SlotTeacherMismatch => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.CannotBookOwnSlot => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.BundlePurchaseNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.BundleExhausted => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.BundleExpired => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPatch("{id:int}/confirm")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> ConfirmBooking([FromRoute] int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.BookingNotFound, "Prenotazione non trovata."));

        var authResult = await _authorizationService.AuthorizeAsync(
            User, new OwnedResourceWrapper(booking.TeacherId), "TeacherOwnsBooking");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _confirmBookingHandler.HandleAsync(id);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.BookingInvalidTransition => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpPatch("{id:int}/cancel")]
    [Authorize]
    public async Task<IActionResult> CancelBooking(
        [FromRoute] int id, [FromBody] CancelBookingRequest request)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.BookingNotFound, "Prenotazione non trovata."));

        var resource = new BookingParticipantResource
            { TeacherId = booking.TeacherId, StudentId = booking.StudentId };
        var authResult = await _authorizationService.AuthorizeAsync(
            User, resource, "BookingParticipant");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var cancellerId = GetCurrentUserId();
        var result = await _cancelBookingHandler.HandleAsync(
            id, request, cancellerId);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.BookingInvalidTransition => StatusCode(
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

    [HttpPatch("{id:int}/complete")]
    [Authorize(Policy = "IsTeacher")]
    public async Task<IActionResult> CompleteBooking([FromRoute] int id)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.BookingNotFound, "Prenotazione non trovata."));

        var authResult = await _authorizationService.AuthorizeAsync(
            User, new OwnedResourceWrapper(booking.TeacherId), "TeacherOwnsBooking");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var result = await _completeBookingHandler.HandleAsync(id);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.BookingInvalidTransition => StatusCode(
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

    [HttpPost("{id:int}/rating")]
    [Authorize]
    public async Task<IActionResult> CreateRating(
        [FromRoute] int id, [FromBody] CreateRatingRequest request)
    {
        var booking = await _bookingRepository.GetByIdAsync(id);
        if (booking is null)
            return StatusCode(
                StatusCodes.Status404NotFound,
                new ApiError(ErrorCodes.BookingNotFound, "Prenotazione non trovata."));

        var authResult = await _authorizationService.AuthorizeAsync(
            User, new OwnedResourceWrapper(booking.StudentId), "StudentOwnsBooking");
        if (!authResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.Forbidden, "Accesso negato."));

        var ratingResource = new RatingWindowResource
        {
            StudentId = booking.StudentId,
            Stato = booking.Stato,
            UpdatedAt = booking.UpdatedAt
        };
        var windowResult = await _authorizationService.AuthorizeAsync(
            User, ratingResource, "RatingWindow");
        if (!windowResult.Succeeded)
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiError(ErrorCodes.RatingWindowExpired,
                    "Rating window has expired or booking is not completed."));

        var userId = GetCurrentUserId();
        var result = await _createRatingHandler.HandleAsync(id, userId, request);
        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.BookingNotFound => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.RatingAlreadyExists => StatusCode(
                    StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.RatingWindowExpired => StatusCode(
                    StatusCodes.Status403Forbidden,
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

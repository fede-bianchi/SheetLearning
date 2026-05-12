using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.UseCases.Payments;
using MusicApp.Web.Models;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly CreateProCheckoutHandler _createProCheckoutHandler;
    private readonly CreateBundleCheckoutHandler _createBundleCheckoutHandler;

    public PaymentsController(
        CreateProCheckoutHandler createProCheckoutHandler,
        CreateBundleCheckoutHandler createBundleCheckoutHandler)
    {
        _createProCheckoutHandler = createProCheckoutHandler;
        _createBundleCheckoutHandler = createBundleCheckoutHandler;
    }

    [HttpPost("checkout/pro")]
    [Authorize]
    public async Task<IActionResult> CheckoutPro([FromBody] CheckoutRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _createProCheckoutHandler.HandleAsync(userId, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.ActiveSubscriptionExists => StatusCode(
                    StatusCodes.Status409Conflict,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                _ => StatusCode(
                    StatusCodes.Status400BadRequest,
                    new ApiError(result.ErrorCode ?? "BAD_REQUEST",
                        result.ErrorMessage ?? "Richiesta non valida."))
            };
        }

        return Ok(result.Value);
    }

    [HttpPost("checkout/bundle/{id:int}")]
    [Authorize]
    public async Task<IActionResult> CheckoutBundle(
        [FromRoute] int id, [FromBody] CheckoutRequest request)
    {
        var userId = GetCurrentUserId();
        var result = await _createBundleCheckoutHandler.HandleAsync(
            userId, id, request);

        if (!result.IsSuccess)
        {
            return result.ErrorCode switch
            {
                ErrorCodes.BundleNotFoundForCheckout => StatusCode(
                    StatusCodes.Status404NotFound,
                    new ApiError(result.ErrorCode!, result.ErrorMessage!)),
                ErrorCodes.BundleInactive => StatusCode(
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

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue("sub")!);
    }
}

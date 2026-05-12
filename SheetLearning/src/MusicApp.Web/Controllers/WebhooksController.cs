using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MusicApp.Application.Common;
using MusicApp.Application.Options;
using MusicApp.Application.UseCases.Payments;
using MusicApp.Web.Models;
using Stripe;

namespace MusicApp.Web.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly HandleStripeWebhookHandler _handler;
    private readonly IOptions<StripeOptions> _stripeOptions;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(
        HandleStripeWebhookHandler handler,
        IOptions<StripeOptions> stripeOptions,
        ILogger<WebhooksController> logger)
    {
        _handler = handler;
        _stripeOptions = stripeOptions;
        _logger = logger;
    }

    [HttpPost("stripe")]
    [AllowAnonymous]
    public async Task<IActionResult> StripeWebhook()
    {
        string payload;
        using (var reader = new StreamReader(
            HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            payload = await reader.ReadToEndAsync();
        }

        var signature = Request.Headers["Stripe-Signature"].ToString();
        if (string.IsNullOrEmpty(signature))
            return BadRequest(new ApiError(
                ErrorCodes.WebhookSignatureInvalid,
                "Missing Stripe-Signature header."));

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                payload,
                signature,
                _stripeOptions.Value.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(
                "Stripe webhook signature validation failed: {Message}",
                ex.Message);
            return BadRequest(new ApiError(
                ErrorCodes.WebhookSignatureInvalid,
                "Invalid Stripe-Signature."));
        }

        await _handler.HandleAsync(stripeEvent);
        return Ok();
    }
}

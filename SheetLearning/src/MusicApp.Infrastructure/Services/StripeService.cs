using Microsoft.Extensions.Options;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Options;
using Stripe;
using Stripe.Checkout;

namespace MusicApp.Infrastructure.Services;

public class StripeService : IStripeService
{
    private readonly IOptions<StripeOptions> _stripeOptions;

    public StripeService(IOptions<StripeOptions> stripeOptions)
    {
        _stripeOptions = stripeOptions;
    }

    public async Task<(string SessionUrl, string SessionId)>
        CreateCheckoutSessionAsync(
            string productName,
            decimal amount,
            string currency,
            string successUrl,
            string cancelUrl,
            Dictionary<string, string> metadata)
    {
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = (long)Math.Round(amount * 100),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = productName
                        }
                    },
                    Quantity = 1
                }
            },
            SuccessUrl = successUrl,
            CancelUrl = cancelUrl,
            Metadata = metadata
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);
        return (session.Url, session.Id);
    }

    public Event ConstructWebhookEvent(string payload, string signature)
    {
        return EventUtility.ConstructEvent(
            payload,
            signature,
            _stripeOptions.Value.WebhookSecret);
    }
}

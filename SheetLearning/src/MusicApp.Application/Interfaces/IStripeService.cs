namespace MusicApp.Application.Interfaces;

public interface IStripeService
{
    Task<(string SessionUrl, string SessionId)> CreateCheckoutSessionAsync(
        string productName,
        decimal amount,
        string currency,
        string successUrl,
        string cancelUrl,
        Dictionary<string, string> metadata);

    Stripe.Event ConstructWebhookEvent(string payload, string signature);
}

namespace MusicApp.Application.Options;

public class StripeOptions
{
    public const string SectionName = "Stripe";
    public string SecretKey     { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
}

using Microsoft.Extensions.Options;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Application.Options;

namespace MusicApp.Application.UseCases.Payments;

public class CreateProCheckoutHandler
{
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IStripeService _stripeService;
    private readonly IOptions<PlansOptions> _options;

    public CreateProCheckoutHandler(
        ISubscriptionRepository subscriptionRepo,
        IPaymentRepository paymentRepo,
        IStripeService stripeService,
        IOptions<PlansOptions> options)
    {
        _subscriptionRepo = subscriptionRepo;
        _paymentRepo = paymentRepo;
        _stripeService = stripeService;
        _options = options;
    }

    public async Task<Result<CheckoutSessionDto>> HandleAsync(
        int userId, CheckoutRequest request)
    {
        var active = await _subscriptionRepo.GetActiveByUserAsync(userId);
        if (active != null)
            return Result<CheckoutSessionDto>.Fail(
                ErrorCodes.ActiveSubscriptionExists,
                "An active Pro subscription already exists.");

        var payment = await _paymentRepo.CreateAsync(new Payment
        {
            UserId = userId,
            Importo = _options.Value.ProMonthlyPriceEur,
            Valuta = "EUR",
            Stato = "pending",
            Tipo = "abbonamento_pro",
            MetodoPagamento = "card",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var metadata = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "tipo", "abbonamento_pro" },
            { "paymentId", payment.Id.ToString() }
        };

        var (sessionUrl, sessionId) = await _stripeService.CreateCheckoutSessionAsync(
            productName: "Piano Pro -- 1 mese",
            amount: _options.Value.ProMonthlyPriceEur,
            currency: "eur",
            successUrl: request.SuccessUrl,
            cancelUrl: request.CancelUrl,
            metadata: metadata);

        payment.RiferimentoEsterno = sessionId;
        await _paymentRepo.UpdateAsync(payment);

        return Result<CheckoutSessionDto>.Ok(new CheckoutSessionDto(sessionUrl));
    }
}

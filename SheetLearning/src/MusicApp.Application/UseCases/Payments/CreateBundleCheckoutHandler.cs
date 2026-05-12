using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Payments;

public class CreateBundleCheckoutHandler
{
    private readonly ILessonBundleRepository _bundleRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IStripeService _stripeService;

    public CreateBundleCheckoutHandler(
        ILessonBundleRepository bundleRepo,
        IPaymentRepository paymentRepo,
        IStripeService stripeService)
    {
        _bundleRepo = bundleRepo;
        _paymentRepo = paymentRepo;
        _stripeService = stripeService;
    }

    public async Task<Result<CheckoutSessionDto>> HandleAsync(
        int userId, int bundleId, CheckoutRequest request)
    {
        var bundle = await _bundleRepo.GetByIdAsync(bundleId);
        if (bundle == null)
            return Result<CheckoutSessionDto>.Fail(
                ErrorCodes.BundleNotFoundForCheckout, "Bundle non trovato.");
        if (!bundle.IsActive)
            return Result<CheckoutSessionDto>.Fail(
                ErrorCodes.BundleInactive, "Bundle non attivo.");

        decimal effectivePrice = bundle.Prezzo
            * (1 - bundle.ScontoPercentuale / 100m);

        var payment = await _paymentRepo.CreateAsync(new Payment
        {
            UserId = userId,
            Importo = effectivePrice,
            Valuta = "EUR",
            Stato = "pending",
            Tipo = "bundle_lezioni",
            MetodoPagamento = "card",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var metadata = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "tipo", "bundle_lezioni" },
            { "bundleId", bundleId.ToString() },
            { "paymentId", payment.Id.ToString() }
        };

        var (sessionUrl, sessionId) = await _stripeService.CreateCheckoutSessionAsync(
            productName: bundle.Nome,
            amount: effectivePrice,
            currency: "eur",
            successUrl: request.SuccessUrl,
            cancelUrl: request.CancelUrl,
            metadata: metadata);

        payment.RiferimentoEsterno = sessionId;
        await _paymentRepo.UpdateAsync(payment);

        return Result<CheckoutSessionDto>.Ok(new CheckoutSessionDto(sessionUrl));
    }
}

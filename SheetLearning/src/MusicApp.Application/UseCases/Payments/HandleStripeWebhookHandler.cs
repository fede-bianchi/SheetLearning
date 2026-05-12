using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Payments;

public class HandleStripeWebhookHandler
{
    private readonly IPaymentRepository _paymentRepo;
    private readonly ISubscriptionRepository _subscriptionRepo;
    private readonly ILessonBundleRepository _bundleRepo;
    private readonly ILessonBundlePurchaseRepository _bundlePurchaseRepo;
    private readonly IUserRepository _userRepo;

    public HandleStripeWebhookHandler(
        IPaymentRepository paymentRepo,
        ISubscriptionRepository subscriptionRepo,
        ILessonBundleRepository bundleRepo,
        ILessonBundlePurchaseRepository bundlePurchaseRepo,
        IUserRepository userRepo)
    {
        _paymentRepo = paymentRepo;
        _subscriptionRepo = subscriptionRepo;
        _bundleRepo = bundleRepo;
        _bundlePurchaseRepo = bundlePurchaseRepo;
        _userRepo = userRepo;
    }

    public async Task<Result<bool>> HandleAsync(Stripe.Event stripeEvent)
    {
        if (stripeEvent.Type != "checkout.session.completed")
            return Result<bool>.Ok(true);

        var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null)
            return Result<bool>.Ok(true);

        return await HandleCheckoutCompletedAsync(session);
    }

    private async Task<Result<bool>> HandleCheckoutCompletedAsync(
        Stripe.Checkout.Session session)
    {
        var payment = await _paymentRepo
            .GetByReferimentoEsternoAsync(session.Id);
        if (payment == null)
            return Result<bool>.Ok(true);

        if (payment.Stato == "completed")
            return Result<bool>.Ok(true);

        payment.Stato = "completed";
        payment.UpdatedAt = DateTime.UtcNow;
        await _paymentRepo.UpdateAsync(payment);

        session.Metadata.TryGetValue("userId", out var userIdStr);
        session.Metadata.TryGetValue("tipo", out var tipo);
        session.Metadata.TryGetValue("bundleId", out var bundleIdStr);

        if (!int.TryParse(userIdStr, out var userId))
            return Result<bool>.Ok(true);

        if (tipo == "abbonamento_pro")
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var datFine = today.AddDays(30);

            await _subscriptionRepo.CreateAsync(new Subscription
            {
                UserId = userId,
                PlanId = 2,
                PaymentId = payment.Id,
                DataInizio = today,
                DataFine = datFine,
                IsActive = true,
                RinnovoAutomatico = true
            });

            var user = await _userRepo.GetByIdAsync(userId);
            if (user != null)
            {
                user.PlanId = 2;
                await _userRepo.UpdateAsync(user);
            }
        }
        else if (tipo == "bundle_lezioni"
            && int.TryParse(bundleIdStr, out var bundleId))
        {
            var bundle = await _bundleRepo.GetByIdAsync(bundleId);
            if (bundle == null)
                return Result<bool>.Ok(true);

            DateTime? expiresAt = bundle.ExpiresAfterDays.HasValue
                ? DateTime.UtcNow.AddDays(bundle.ExpiresAfterDays.Value)
                : null;

            await _bundlePurchaseRepo.CreateAsync(
                new LessonBundlePurchase
                {
                    UserId = userId,
                    BundleId = bundleId,
                    LezioniTotali = bundle.NumeroLezioni,
                    LezioniUsate = 0,
                    PaymentId = payment.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = expiresAt
                });
        }

        return Result<bool>.Ok(true);
    }
}

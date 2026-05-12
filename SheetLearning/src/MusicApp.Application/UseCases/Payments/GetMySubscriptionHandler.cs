using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Payments;

public class GetMySubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepo;

    public GetMySubscriptionHandler(ISubscriptionRepository subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<Result<SubscriptionDto>> HandleAsync(int userId)
    {
        var sub = await _subscriptionRepo.GetActiveByUserAsync(userId);
        if (sub is null)
            return Result<SubscriptionDto>.Fail(
                ErrorCodes.NoActiveSubscription,
                "Nessun abbonamento attivo trovato.");

        var dto = new SubscriptionDto(
            sub.Id,
            sub.PlanId,
            sub.Plan?.Nome ?? "Pro",
            sub.DataInizio,
            sub.DataFine,
            sub.IsActive,
            sub.RinnovoAutomatico
        );

        return Result<SubscriptionDto>.Ok(dto);
    }
}

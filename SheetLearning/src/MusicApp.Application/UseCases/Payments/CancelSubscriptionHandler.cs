using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Payments;

public class CancelSubscriptionHandler
{
    private readonly ISubscriptionRepository _subscriptionRepo;

    public CancelSubscriptionHandler(ISubscriptionRepository subscriptionRepo)
    {
        _subscriptionRepo = subscriptionRepo;
    }

    public async Task<Result<bool>> HandleAsync(int userId)
    {
        var sub = await _subscriptionRepo.GetActiveByUserAsync(userId);
        if (sub is null)
            return Result<bool>.Fail(
                ErrorCodes.NoActiveSubscription,
                "Nessun abbonamento attivo trovato.");

        sub.RinnovoAutomatico = false;
        await _subscriptionRepo.UpdateAsync(sub);

        return Result<bool>.Ok(true);
    }
}

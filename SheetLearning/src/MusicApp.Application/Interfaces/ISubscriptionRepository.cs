using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetActiveByUserAsync(int userId);
    Task<Subscription> CreateAsync(Subscription subscription);
    Task<Subscription> UpdateAsync(Subscription subscription);
}

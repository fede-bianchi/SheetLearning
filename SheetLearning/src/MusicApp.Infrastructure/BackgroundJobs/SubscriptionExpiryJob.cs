using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicApp.Application.Interfaces;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.BackgroundJobs;

public class SubscriptionExpiryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SubscriptionExpiryJob> _logger;

    public SubscriptionExpiryJob(
        IServiceScopeFactory scopeFactory,
        ILogger<SubscriptionExpiryJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        do
        {
            try
            {
                await RunAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubscriptionExpiryJob encountered an error");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var expired = await context.Subscriptions
            .Include(s => s.User)
            .Where(s => s.IsActive && s.DataFine < today)
            .ToListAsync(ct);

        foreach (var sub in expired)
        {
            sub.IsActive = false;
            sub.User.PlanId = 1;
            context.Subscriptions.Update(sub);
            context.Users.Update(sub.User);
        }
        await context.SaveChangesAsync(ct);

        _logger.LogInformation("Downgraded {Count} expired subscriptions", expired.Count);

        var inSevenDays = today.AddDays(7);
        var approaching = await context.Subscriptions
            .Where(s => s.IsActive && s.DataFine == inSevenDays)
            .ToListAsync(ct);

        foreach (var sub in approaching)
        {
            try
            {
                await notificationService.SendAbbonamentoInScadenzaAsync(
                    sub.UserId, sub.DataFine, sub.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send expiry notification for subscription {SubId}", sub.Id);
            }
        }

        _logger.LogInformation("Sent {Count} expiry-approaching notifications", approaching.Count);
    }
}

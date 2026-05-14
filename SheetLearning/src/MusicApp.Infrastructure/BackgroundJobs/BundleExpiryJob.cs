using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MusicApp.Application.Interfaces;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.BackgroundJobs;

public class BundleExpiryJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BundleExpiryJob> _logger;

    public BundleExpiryJob(
        IServiceScopeFactory scopeFactory,
        ILogger<BundleExpiryJob> logger)
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
                _logger.LogError(ex, "BundleExpiryJob encountered an error");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var sevenDaysFromNow = DateTime.UtcNow.AddDays(7);
        var dayStart = sevenDaysFromNow.Date;
        var dayEnd   = dayStart.AddDays(1);

        var expiring = await context.LessonBundlePurchases
            .Include(p => p.Bundle)
            .Where(p => p.ExpiresAt.HasValue
                     && p.ExpiresAt.Value >= dayStart
                     && p.ExpiresAt.Value <  dayEnd
                     && p.LezioniUsate < p.LezioniTotali)
            .ToListAsync(ct);

        foreach (var purchase in expiring)
        {
            try
            {
                await notificationService.SendBundleInScadenzaAsync(
                    purchase.UserId,
                    purchase.Bundle.Nome,
                    purchase.LezioniTotali - purchase.LezioniUsate,
                    DateOnly.FromDateTime(purchase.ExpiresAt!.Value),
                    purchase.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send bundle expiry notification for purchase {PurchaseId}", purchase.Id);
            }
        }

        _logger.LogInformation("Sent {Count} bundle-expiry notifications", expiring.Count);
    }
}

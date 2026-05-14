using Microsoft.EntityFrameworkCore;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class AdminStatsRepository : IAdminStatsRepository
{
    private readonly AppDbContext _context;

    public AdminStatsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PlatformStatsDto> GetPlatformOverviewAsync()
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        return new PlatformStatsDto(
            TotaleUtentiAttivi      : await _context.Users.CountAsync(u => u.IsActive),
            TotaleUtentiBannati     : await _context.Users.CountAsync(u => !u.IsActive),
            TotaleInsegnanti        : await _context.Users
                                        .CountAsync(u => u.Role.Nome == "Insegnante"),
            TotaleUtentiPro         : await _context.Users
                                        .CountAsync(u => u.Plan.Nome == "Pro"),
            TotalePost              : await _context.Posts.CountAsync(),
            TotaleCommenti          : await _context.Comments.CountAsync(),
            TotalePrenotazioni      : await _context.LessonBookings.CountAsync(),
            TotaleTentativi         : await _context.Attempts.CountAsync(),
            TotaleIscrizioniAttive  : await _context.Subscriptions
                                        .CountAsync(s => s.IsActive &&
                                            s.DataFine >= DateOnly.FromDateTime(DateTime.UtcNow)),
            EntrateUltimoMese       : await _context.Payments
                                        .Where(p => p.Stato == "completed"
                                                 && p.CreatedAt >= thirtyDaysAgo)
                                        .SumAsync(p => (decimal?)p.Importo) ?? 0m
        );
    }

    public async Task<RevenueStatsDto> GetRevenueAsync(DateOnly dal, DateOnly al)
    {
        var dalDt = dal.ToDateTime(TimeOnly.MinValue);
        var alDt  = al.ToDateTime(TimeOnly.MaxValue);

        var payments = await _context.Payments
            .Where(p => p.CreatedAt >= dalDt && p.CreatedAt <= alDt)
            .ToListAsync();

        var completed = payments.Where(p => p.Stato == "completed").ToList();
        var byTipo    = completed
            .GroupBy(p => p.Tipo)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Importo));

        return new RevenueStatsDto(
            PeriodoDal            : dal,
            PeriodoAl             : al,
            TotaleEntrate         : completed.Sum(p => p.Importo),
            EntratePerTipo        : byTipo,
            PagamentiCompletati   : completed.Count,
            PagamentiFalliti      : payments.Count(p => p.Stato == "failed"),
            PagamentiPending      : payments.Count(p => p.Stato == "pending")
        );
    }
}

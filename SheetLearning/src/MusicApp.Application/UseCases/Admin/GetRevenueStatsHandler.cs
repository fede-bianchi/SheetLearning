using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetRevenueStatsHandler
{
    private readonly IAdminStatsRepository _adminStatsRepository;

    public GetRevenueStatsHandler(IAdminStatsRepository adminStatsRepository)
    {
        _adminStatsRepository = adminStatsRepository;
    }

    public async Task<Result<RevenueStatsDto>> HandleAsync(DateRangeQuery query)
    {
        var al = query.Al ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var dal = query.Dal ?? al.AddDays(-30);

        if (dal > al)
            return Result<RevenueStatsDto>.Fail(ErrorCodes.InvalidDateRange, "Dal must be ≤ Al.");

        var dto = await _adminStatsRepository.GetRevenueAsync(dal, al);
        return Result<RevenueStatsDto>.Ok(dto);
    }
}

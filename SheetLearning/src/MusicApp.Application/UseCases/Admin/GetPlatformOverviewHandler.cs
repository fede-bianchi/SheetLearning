using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetPlatformOverviewHandler
{
    private readonly IAdminStatsRepository _adminStatsRepository;

    public GetPlatformOverviewHandler(IAdminStatsRepository adminStatsRepository)
    {
        _adminStatsRepository = adminStatsRepository;
    }

    public async Task<Result<PlatformStatsDto>> HandleAsync()
    {
        var dto = await _adminStatsRepository.GetPlatformOverviewAsync();
        return Result<PlatformStatsDto>.Ok(dto);
    }
}

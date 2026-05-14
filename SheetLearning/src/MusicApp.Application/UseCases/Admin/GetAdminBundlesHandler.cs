using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetAdminBundlesHandler
{
    private readonly ILessonBundleRepository _bundleRepository;

    public GetAdminBundlesHandler(ILessonBundleRepository bundleRepository)
    {
        _bundleRepository = bundleRepository;
    }

    public async Task<Result<IReadOnlyList<LessonBundleDto>>> HandleAsync()
    {
        var bundles = await _bundleRepository.GetAllAsync();

        var dtos = bundles.Select(b => new LessonBundleDto(
            b.Id,
            b.Nome,
            b.NumeroLezioni,
            b.Prezzo,
            b.ScontoPercentuale,
            b.IsActive,
            b.ExpiresAfterDays
        )).ToList();

        return Result<IReadOnlyList<LessonBundleDto>>.Ok(dtos);
    }
}

using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class UpdateBundleHandler
{
    private readonly ILessonBundleRepository _bundleRepository;

    public UpdateBundleHandler(ILessonBundleRepository bundleRepository)
    {
        _bundleRepository = bundleRepository;
    }

    public async Task<Result<LessonBundleDto>> HandleAsync(int bundleId, UpdateBundleRequest request)
    {
        var bundle = await _bundleRepository.GetByIdAsync(bundleId);
        if (bundle is null)
            return Result<LessonBundleDto>.Fail(ErrorCodes.BundleNotFound, "Bundle non trovato.");

        bundle.Nome              = request.NomeBundle;
        bundle.NumeroLezioni     = request.NumeroLezioni;
        bundle.Prezzo            = request.Prezzo;
        bundle.ScontoPercentuale = request.ScontoPercentuale;
        bundle.ExpiresAfterDays  = request.ExpiresAfterDays;

        bundle = await _bundleRepository.UpdateAsync(bundle);

        return Result<LessonBundleDto>.Ok(new LessonBundleDto(
            bundle.Id,
            bundle.Nome,
            bundle.NumeroLezioni,
            bundle.Prezzo,
            bundle.ScontoPercentuale,
            bundle.IsActive,
            bundle.ExpiresAfterDays
        ));
    }
}

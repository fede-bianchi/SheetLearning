using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class ToggleBundleHandler
{
    private readonly ILessonBundleRepository _bundleRepository;

    public ToggleBundleHandler(ILessonBundleRepository bundleRepository)
    {
        _bundleRepository = bundleRepository;
    }

    public async Task<Result<bool>> HandleAsync(int bundleId)
    {
        var bundle = await _bundleRepository.GetByIdAsync(bundleId);
        if (bundle is null)
            return Result<bool>.Fail(ErrorCodes.BundleNotFound, "Bundle non trovato.");

        bundle.IsActive = !bundle.IsActive;
        await _bundleRepository.UpdateAsync(bundle);

        return Result<bool>.Ok(true);
    }
}

using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Admin;

public class CreateBundleHandler
{
    private readonly ILessonBundleRepository _bundleRepository;

    public CreateBundleHandler(ILessonBundleRepository bundleRepository)
    {
        _bundleRepository = bundleRepository;
    }

    public async Task<Result<LessonBundleDto>> HandleAsync(CreateBundleRequest request)
    {
        var bundle = await _bundleRepository.CreateAsync(new LessonBundle
        {
            Nome              = request.NomeBundle,
            NumeroLezioni     = request.NumeroLezioni,
            Prezzo            = request.Prezzo,
            ScontoPercentuale = request.ScontoPercentuale,
            IsActive          = true,
            ExpiresAfterDays  = request.ExpiresAfterDays,
            CreatedAt         = DateTime.UtcNow
        });

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

using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Bundles;

public class GetMyBundlePurchasesHandler
{
    private readonly ILessonBundlePurchaseRepository _bundlePurchaseRepository;

    public GetMyBundlePurchasesHandler(
        ILessonBundlePurchaseRepository bundlePurchaseRepository)
    {
        _bundlePurchaseRepository = bundlePurchaseRepository;
    }

    public async Task<Result<IReadOnlyList<BundlePurchaseDto>>> HandleAsync(int userId)
    {
        var purchases = await _bundlePurchaseRepository.GetByUserAsync(userId);

        var dtos = purchases.Select(p => new BundlePurchaseDto(
            p.Id,
            p.BundleId,
            p.Bundle?.Nome ?? string.Empty,
            p.LezioniTotali,
            p.LezioniUsate,
            p.LezioniTotali - p.LezioniUsate,
            p.CreatedAt,
            p.ExpiresAt
        )).ToList();

        return Result<IReadOnlyList<BundlePurchaseDto>>.Ok(dtos);
    }
}

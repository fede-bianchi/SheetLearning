using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface ILessonBundlePurchaseRepository
{
    Task<IReadOnlyList<LessonBundlePurchase>> GetByUserAsync(int userId);
    Task<LessonBundlePurchase?> GetByIdAsync(int id);
    Task<LessonBundlePurchase> CreateAsync(LessonBundlePurchase purchase);
    Task<LessonBundlePurchase> UpdateAsync(LessonBundlePurchase purchase);
}

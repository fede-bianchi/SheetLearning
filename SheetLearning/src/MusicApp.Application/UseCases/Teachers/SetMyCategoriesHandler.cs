using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Teachers;

public class SetMyCategoriesHandler
{
    private readonly ITeacherProfileRepository _teacherProfileRepository;

    public SetMyCategoriesHandler(ITeacherProfileRepository teacherProfileRepository)
    {
        _teacherProfileRepository = teacherProfileRepository;
    }

    public async Task<Result<bool>> HandleAsync(
        int userId, SetCategoriesRequest request)
    {
        var profile = await _teacherProfileRepository.GetByUserIdAsync(userId);
        if (profile is null)
            return Result<bool>.Fail(
                ErrorCodes.TeacherProfileNotFound, "Profilo insegnante non trovato.");

        var deduped = request.Categorie.Distinct().ToArray();

        if (deduped.Length < 1 || deduped.Length > 3)
            return Result<bool>.Fail(
                ErrorCodes.InvalidCategoryCount,
                "Devi selezionare da 1 a 3 categorie distinte.");

        await _teacherProfileRepository.SetCategoriesAsync(profile.Id, deduped);

        return Result<bool>.Ok(true);
    }
}

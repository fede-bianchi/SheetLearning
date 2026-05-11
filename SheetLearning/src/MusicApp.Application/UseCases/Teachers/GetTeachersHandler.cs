using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Teachers;

public class GetTeachersHandler
{
    private readonly ITeacherProfileRepository _teacherProfileRepository;

    public GetTeachersHandler(ITeacherProfileRepository teacherProfileRepository)
    {
        _teacherProfileRepository = teacherProfileRepository;
    }

    public async Task<Result<IReadOnlyList<TeacherSummaryDto>>> HandleAsync(
        TeacherListQuery query, string? planClaim, string? roleClaim)
    {
        var profiles = await _teacherProfileRepository.GetVisibleAsync(
            planClaim, roleClaim, query.FiltroCategoria);

        var dtos = profiles.Select(MapToSummary).ToList();
        return Result<IReadOnlyList<TeacherSummaryDto>>.Ok(dtos);
    }

    private static TeacherSummaryDto MapToSummary(TeacherProfile profile)
    {
        return new TeacherSummaryDto(
            profile.Id,
            profile.UserId,
            profile.User?.Nickname ?? string.Empty,
            profile.User?.Strumento,
            profile.Bio,
            profile.Specializzazioni,
            profile.VisibileA,
            profile.Categories.Select(c => c.Categoria).ToArray()
        );
    }
}

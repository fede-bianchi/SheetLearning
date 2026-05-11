using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Teachers;

public class GetTeacherPublicProfileHandler
{
    private readonly ITeacherProfileRepository _teacherProfileRepository;

    public GetTeacherPublicProfileHandler(ITeacherProfileRepository teacherProfileRepository)
    {
        _teacherProfileRepository = teacherProfileRepository;
    }

    public async Task<Result<TeacherProfileDto>> HandleAsync(
        int teacherProfileId, string? planClaim, string? roleClaim)
    {
        var profile = await _teacherProfileRepository.GetByIdAsync(teacherProfileId);
        if (profile is null)
            return Result<TeacherProfileDto>.Fail(
                ErrorCodes.TeacherProfileNotFound, "Profilo insegnante non trovato.");

        if (profile.VisibileA == "nessuno")
            return Result<TeacherProfileDto>.Fail(
                ErrorCodes.TeacherNotVisible, "Insegnante non visibile.");

        if (profile.VisibileA == "solo_pro"
            && planClaim != "Pro"
            && roleClaim != "Admin")
        {
            return Result<TeacherProfileDto>.Fail(
                ErrorCodes.TeacherNotVisible, "Insegnante non visibile.");
        }

        var dto = MapToProfile(profile);
        return Result<TeacherProfileDto>.Ok(dto);
    }

    private static TeacherProfileDto MapToProfile(TeacherProfile profile)
    {
        return new TeacherProfileDto(
            profile.Id,
            profile.UserId,
            profile.User?.Nickname ?? string.Empty,
            profile.User?.Strumento,
            profile.User?.Descrizione,
            profile.Bio,
            profile.Specializzazioni,
            profile.VisibileA,
            profile.Categories.Select(c => c.Categoria).ToArray(),
            profile.CreatedAt
        );
    }
}

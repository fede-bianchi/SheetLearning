using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Teachers;

public class GetMyTeacherProfileHandler
{
    private readonly ITeacherProfileRepository _teacherProfileRepository;

    public GetMyTeacherProfileHandler(ITeacherProfileRepository teacherProfileRepository)
    {
        _teacherProfileRepository = teacherProfileRepository;
    }

    public async Task<Result<TeacherProfileDto>> HandleAsync(int userId)
    {
        var profile = await _teacherProfileRepository.GetByUserIdAsync(userId);
        if (profile is null)
            return Result<TeacherProfileDto>.Fail(
                ErrorCodes.TeacherProfileNotFound, "Profilo insegnante non trovato.");

        var dto = new TeacherProfileDto(
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

        return Result<TeacherProfileDto>.Ok(dto);
    }
}

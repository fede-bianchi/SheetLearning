using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Teachers;

public class GetMyStudentsHandler
{
    private readonly ILessonBookingRepository _bookingRepository;

    public GetMyStudentsHandler(ILessonBookingRepository bookingRepository)
    {
        _bookingRepository = bookingRepository;
    }

    public async Task<Result<IReadOnlyList<StudentSummaryDto>>> HandleAsync(int teacherId)
    {
        var students = await _bookingRepository.GetStudentsByTeacherAsync(teacherId);

        var dtos = students.Select(s => new StudentSummaryDto(
            s.Id,
            s.Nickname,
            s.Nome,
            s.Cognome,
            s.Strumento
        )).ToList();

        return Result<IReadOnlyList<StudentSummaryDto>>.Ok(dtos);
    }
}

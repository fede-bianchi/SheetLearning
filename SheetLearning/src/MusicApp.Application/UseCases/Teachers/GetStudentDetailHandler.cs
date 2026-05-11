using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Teachers;

public class GetStudentDetailHandler
{
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly IUserRepository _userRepository;

    public GetStudentDetailHandler(
        ILessonBookingRepository bookingRepository,
        IUserRepository userRepository)
    {
        _bookingRepository = bookingRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<TeacherStudentDetailDto>> HandleAsync(
        int teacherId, int studentId)
    {
        var student = await _userRepository.GetByIdAsync(studentId);
        if (student is null)
            return Result<TeacherStudentDetailDto>.Fail(
                ErrorCodes.StudentNotFound, "Studente non trovato.");

        var totaleLezioni = await _bookingRepository.CountCompletedAsync(
            teacherId, studentId);

        var lastCompleted = await _bookingRepository.GetLastCompletedAsync(
            teacherId, studentId);

        var dto = new TeacherStudentDetailDto(
            student.Id,
            student.Nickname,
            student.Nome,
            student.Cognome,
            student.Strumento,
            totaleLezioni,
            lastCompleted?.UpdatedAt
        );

        return Result<TeacherStudentDetailDto>.Ok(dto);
    }
}

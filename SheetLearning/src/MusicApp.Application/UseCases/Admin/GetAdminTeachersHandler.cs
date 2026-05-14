using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetAdminTeachersHandler
{
    private readonly ITeacherProfileRepository _teacherProfileRepository;
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly ILessonRatingRepository _ratingRepository;

    public GetAdminTeachersHandler(
        ITeacherProfileRepository teacherProfileRepository,
        ILessonBookingRepository bookingRepository,
        ILessonRatingRepository ratingRepository)
    {
        _teacherProfileRepository = teacherProfileRepository;
        _bookingRepository = bookingRepository;
        _ratingRepository = ratingRepository;
    }

    public async Task<Result<IReadOnlyList<AdminTeacherDto>>> HandleAsync()
    {
        var profiles = await _teacherProfileRepository.GetAllWithUserAsync();

        var dtos = new List<AdminTeacherDto>();

        foreach (var tp in profiles)
        {
            var totalePrenotazioni = await _bookingRepository.CountByTeacherAsync(tp.UserId);
            var prenotazioniCompletate = await _bookingRepository.CountCompletedByTeacherAsync(tp.UserId);
            var ratings = await _ratingRepository.GetByTeacherAsync(tp.UserId);
            var mediaVal = ratings.Count > 0
                ? Math.Round((decimal)ratings.Sum(r => r.Valutazione) / ratings.Count, 2)
                : 0m;

            dtos.Add(new AdminTeacherDto(
                tp.Id,
                tp.UserId,
                tp.User.Nickname,
                tp.User.Nome,
                tp.User.Cognome,
                tp.User.Strumento,
                tp.VisibileA,
                tp.Categories.Select(c => c.Categoria).ToArray(),
                totalePrenotazioni,
                prenotazioniCompletate,
                mediaVal,
                ratings.Count
            ));
        }

        return Result<IReadOnlyList<AdminTeacherDto>>.Ok(dtos);
    }
}

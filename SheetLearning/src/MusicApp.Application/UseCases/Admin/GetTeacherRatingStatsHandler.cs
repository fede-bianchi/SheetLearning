using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetTeacherRatingStatsHandler
{
    private readonly ITeacherProfileRepository _teacherProfileRepository;
    private readonly ILessonRatingRepository _ratingRepository;

    public GetTeacherRatingStatsHandler(
        ITeacherProfileRepository teacherProfileRepository,
        ILessonRatingRepository ratingRepository)
    {
        _teacherProfileRepository = teacherProfileRepository;
        _ratingRepository = ratingRepository;
    }

    public async Task<Result<TeacherRatingStatsDto>> HandleAsync(int teacherProfileId)
    {
        var profile = await _teacherProfileRepository.GetByIdAsync(teacherProfileId);
        if (profile is null)
            return Result<TeacherRatingStatsDto>.Fail(
                ErrorCodes.TeacherProfileNotFound, "Profilo insegnante non trovato.");

        var ratings = await _ratingRepository.GetByTeacherAsync(profile.UserId);

        var total  = ratings.Count;
        var count1 = ratings.Count(r => r.Valutazione == 1);
        var count2 = ratings.Count(r => r.Valutazione == 2);
        var count3 = ratings.Count(r => r.Valutazione == 3);

        var distribuzione = new Dictionary<int, decimal>
        {
            { 1, total > 0 ? Math.Round((decimal)count1 / total, 2) : 0m },
            { 2, total > 0 ? Math.Round((decimal)count2 / total, 2) : 0m },
            { 3, total > 0 ? Math.Round((decimal)count3 / total, 2) : 0m }
        };

        var media = total > 0
            ? Math.Round((decimal)(count1 * 1 + count2 * 2 + count3 * 3) / total, 2)
            : 0m;

        return Result<TeacherRatingStatsDto>.Ok(new TeacherRatingStatsDto(
            profile.Id,
            profile.UserId,
            profile.User.Nickname,
            total,
            distribuzione,
            media
        ));
    }
}

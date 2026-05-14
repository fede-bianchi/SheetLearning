namespace MusicApp.Application.DTOs;

public record TeacherRatingStatsDto(
    int                      TeacherProfileId,
    int                      UserId,
    string                   Nickname,
    int                      TotaleValutazioni,
    Dictionary<int, decimal> Distribuzione,
    decimal                  MediaPonderata
);

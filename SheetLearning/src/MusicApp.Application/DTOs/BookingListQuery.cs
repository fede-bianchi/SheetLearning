namespace MusicApp.Application.DTOs;

public record BookingListQuery(
    DateOnly? Dal,
    DateOnly? Al
);

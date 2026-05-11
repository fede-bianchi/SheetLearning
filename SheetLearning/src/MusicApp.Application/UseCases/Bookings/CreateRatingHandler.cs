using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Bookings;

public class CreateRatingHandler
{
    private readonly ILessonBookingRepository _bookingRepository;
    private readonly ILessonRatingRepository _ratingRepository;

    public CreateRatingHandler(
        ILessonBookingRepository bookingRepository,
        ILessonRatingRepository ratingRepository)
    {
        _bookingRepository = bookingRepository;
        _ratingRepository = ratingRepository;
    }

    public async Task<Result<bool>> HandleAsync(
        int bookingId, int studentId, CreateRatingRequest request)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId);
        if (booking is null)
            return Result<bool>.Fail(
                ErrorCodes.BookingNotFound, "Prenotazione non trovata.");

        var existingRating = await _ratingRepository.GetByBookingAsync(bookingId);
        if (existingRating is not null)
            return Result<bool>.Fail(
                ErrorCodes.RatingAlreadyExists,
                "Hai gia valutato questa lezione.");

        var rating = new LessonRating
        {
            BookingId = bookingId,
            StudentId = studentId,
            TeacherId = booking.TeacherId,
            Valutazione = request.Valutazione,
            CreatedAt = DateTime.UtcNow
        };

        await _ratingRepository.CreateAsync(rating);
        return Result<bool>.Ok(true);
    }
}

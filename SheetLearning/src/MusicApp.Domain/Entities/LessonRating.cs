namespace MusicApp.Domain.Entities;

public class LessonRating
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public byte Valutazione { get; set; }
    public DateTime CreatedAt { get; set; }

    public LessonBooking Booking { get; set; } = null!;
    public User Student { get; set; } = null!;
    public User Teacher { get; set; } = null!;
}

namespace MusicApp.Domain.Entities;

public class LessonBooking
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int SlotId { get; set; }
    public int? BundlePurchaseId { get; set; }
    public string Stato { get; set; } = string.Empty;
    public string? NoteStudente { get; set; }
    public string? NoteInsegnante { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User Student { get; set; } = null!;
    public User Teacher { get; set; } = null!;
    public LessonSlot Slot { get; set; } = null!;
    public LessonBundlePurchase? BundlePurchase { get; set; }
    public LessonRating? Rating { get; set; }
}

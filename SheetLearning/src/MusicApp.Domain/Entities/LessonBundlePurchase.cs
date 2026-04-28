namespace MusicApp.Domain.Entities;

public class LessonBundlePurchase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BundleId { get; set; }
    public int LezioniTotali { get; set; }
    public int LezioniUsate { get; set; }
    public int PaymentId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public User User { get; set; } = null!;
    public LessonBundle Bundle { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
}

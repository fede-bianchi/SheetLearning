namespace MusicApp.Domain.Entities;

public class LessonSlot
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public DateTime DataOraInizio { get; set; }
    public DateTime DataOraFine { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Teacher { get; set; } = null!;
}

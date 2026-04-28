namespace MusicApp.Domain.Entities;

public class Notification
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Titolo { get; set; } = string.Empty;
    public string? Corpo { get; set; }
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public bool IsRead { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public User User { get; set; } = null!;
}

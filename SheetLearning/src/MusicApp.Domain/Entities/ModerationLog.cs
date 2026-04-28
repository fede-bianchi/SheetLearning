namespace MusicApp.Domain.Entities;

public class ModerationLog
{
    public int Id { get; set; }
    public int AdminId { get; set; }
    public int? TargetUserId { get; set; }
    public string Azione { get; set; } = string.Empty;
    public string? TargetType { get; set; }
    public int? TargetId { get; set; }
    public string Motivazione { get; set; } = string.Empty;
    public string? ContenutoRimosso { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Admin { get; set; } = null!;
    public User? TargetUser { get; set; }
}

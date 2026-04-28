namespace MusicApp.Domain.Entities;

public class Vote
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public int TargetId { get; set; }
    public string Voto { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}

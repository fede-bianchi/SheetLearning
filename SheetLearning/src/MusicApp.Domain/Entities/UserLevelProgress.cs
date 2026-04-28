namespace MusicApp.Domain.Entities;

public class UserLevelProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LevelId { get; set; }
    public bool Sbloccato { get; set; }
    public bool Completato { get; set; }
    public DateTime? DataSblocco { get; set; }
    public DateTime? DataCompletamento { get; set; }

    public User User { get; set; } = null!;
    public Level Level { get; set; } = null!;
}

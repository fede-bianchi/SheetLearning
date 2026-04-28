namespace MusicApp.Domain.Entities;

public class TeacherProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Bio { get; set; }
    public string? Specializzazioni { get; set; }
    public string VisibileA { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<TeacherCategory> Categories { get; set; } = new List<TeacherCategory>();
}

namespace MusicApp.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Titolo { get; set; } = string.Empty;
    public string Contenuto { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    public User User { get; set; } = null!;
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

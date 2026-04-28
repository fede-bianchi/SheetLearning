namespace MusicApp.Domain.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string Contenuto { get; set; } = string.Empty;
    public bool Letto { get; set; }
    public DateTime CreatedAt { get; set; }

    public Chat Chat { get; set; } = null!;
    public User Sender { get; set; } = null!;
}

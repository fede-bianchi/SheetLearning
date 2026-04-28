namespace MusicApp.Domain.Entities;

public class LessonBundle
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int NumeroLezioni { get; set; }
    public decimal Prezzo { get; set; }
    public decimal ScontoPercentuale { get; set; }
    public bool IsActive { get; set; }
    public int? ExpiresAfterDays { get; set; }
    public DateTime CreatedAt { get; set; }
}

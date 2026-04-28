namespace MusicApp.Domain.Entities;

public class Attempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ExerciseTypeId { get; set; }
    public int? LevelId { get; set; }
    public byte? Difficolta { get; set; }
    public int Punteggio { get; set; }
    public int? TempoRispostaMs { get; set; }
    public string? ExerciseSubtype { get; set; }
    public int? ToleranceWindowMs { get; set; }
    public string InputSource { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; } = null!;
    public Level? Level { get; set; }
    public ICollection<AttemptError> AttemptErrors { get; set; } = new List<AttemptError>();
}

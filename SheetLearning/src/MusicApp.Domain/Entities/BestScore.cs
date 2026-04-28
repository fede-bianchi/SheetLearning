namespace MusicApp.Domain.Entities;

public class BestScore
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ExerciseTypeId { get; set; }
    public int PunteggioMigliore { get; set; }
    public int AttemptId { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ExerciseType ExerciseType { get; set; } = null!;
    public Attempt Attempt { get; set; } = null!;
}

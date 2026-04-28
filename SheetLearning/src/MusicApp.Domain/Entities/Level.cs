namespace MusicApp.Domain.Entities;

public class Level
{
    public int Id { get; set; }
    public int ExerciseTypeId { get; set; }
    public int? ClefId { get; set; }
    public byte NumeroLivello { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public int PunteggioMinimoSblocco { get; set; }

    public ExerciseType ExerciseType { get; set; } = null!;
    public Clef? Clef { get; set; }
}

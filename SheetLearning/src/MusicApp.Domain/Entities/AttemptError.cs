namespace MusicApp.Domain.Entities;

public class AttemptError
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public string ElementType { get; set; } = string.Empty;
    public string RispostaData { get; set; } = string.Empty;
    public string RispostaCorretta { get; set; } = string.Empty;
    public byte? PosizioneNelPattern { get; set; }
    public DateTime CreatedAt { get; set; }

    public Attempt Attempt { get; set; } = null!;
}

namespace MusicApp.Domain.Entities;

public class Plan
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
    public decimal PrezzoMensile { get; set; }
}

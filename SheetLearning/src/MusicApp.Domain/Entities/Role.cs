namespace MusicApp.Domain.Entities;

public class Role
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descrizione { get; set; }
}

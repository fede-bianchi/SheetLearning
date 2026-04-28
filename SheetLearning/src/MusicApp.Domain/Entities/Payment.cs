namespace MusicApp.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Importo { get; set; }
    public string Valuta { get; set; } = string.Empty;
    public string Stato { get; set; } = string.Empty;
    public string? MetodoPagamento { get; set; }
    public string? RiferimentoEsterno { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}

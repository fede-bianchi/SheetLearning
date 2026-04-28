namespace MusicApp.Domain.Entities;

public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    public int PaymentId { get; set; }
    public DateOnly DataInizio { get; set; }
    public DateOnly DataFine { get; set; }
    public bool IsActive { get; set; }
    public bool RinnovoAutomatico { get; set; }

    public User User { get; set; } = null!;
    public Plan Plan { get; set; } = null!;
    public Payment Payment { get; set; } = null!;
}

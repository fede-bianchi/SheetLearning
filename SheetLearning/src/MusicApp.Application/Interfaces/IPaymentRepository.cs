using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IPaymentRepository
{
    Task<Payment> CreateAsync(Payment payment);
    Task<Payment> UpdateAsync(Payment payment);
    Task<Payment?> GetByReferimentoEsternoAsync(string riferimentoEsterno);
    Task<IReadOnlyList<Payment>> GetByUserAsync(int userId);
}

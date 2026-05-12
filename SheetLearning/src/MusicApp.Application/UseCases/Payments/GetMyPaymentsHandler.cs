using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Payments;

public class GetMyPaymentsHandler
{
    private readonly IPaymentRepository _paymentRepo;

    public GetMyPaymentsHandler(IPaymentRepository paymentRepo)
    {
        _paymentRepo = paymentRepo;
    }

    public async Task<Result<IReadOnlyList<PaymentDto>>> HandleAsync(int userId)
    {
        var payments = await _paymentRepo.GetByUserAsync(userId);

        var dtos = payments.Select(p => new PaymentDto(
            p.Id,
            p.Importo,
            p.Valuta,
            p.Stato,
            p.Tipo,
            p.RiferimentoEsterno,
            p.CreatedAt
        )).ToList();

        return Result<IReadOnlyList<PaymentDto>>.Ok(dtos);
    }
}

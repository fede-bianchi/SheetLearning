namespace MusicApp.Application.DTOs;

public record PaymentDto(
    int      Id,
    decimal  Importo,
    string   Valuta,
    string   Stato,
    string   Tipo,
    string?  RiferimentoEsterno,
    DateTime CreatedAt
);

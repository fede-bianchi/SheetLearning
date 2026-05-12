namespace MusicApp.Application.DTOs;

public record CheckoutRequest(
    string SuccessUrl,
    string CancelUrl
);

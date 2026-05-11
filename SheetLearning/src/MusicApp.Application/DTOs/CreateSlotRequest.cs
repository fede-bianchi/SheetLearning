namespace MusicApp.Application.DTOs;

public record CreateSlotRequest(
    DateTime DataOraInizio,
    DateTime DataOraFine
);

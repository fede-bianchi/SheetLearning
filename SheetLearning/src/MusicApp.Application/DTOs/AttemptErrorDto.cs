namespace MusicApp.Application.DTOs;

public record AttemptErrorDto(
    string ElementType,
    string RispostaData,
    string RispostaCorretta,
    byte? PosizioneNelPattern
);

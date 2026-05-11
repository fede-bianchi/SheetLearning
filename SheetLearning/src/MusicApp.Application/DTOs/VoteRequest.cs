namespace MusicApp.Application.DTOs;

public record VoteRequest(
    string TargetType,
    int TargetId,
    string Voto
);

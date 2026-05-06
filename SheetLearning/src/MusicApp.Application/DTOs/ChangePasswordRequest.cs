namespace MusicApp.Application.DTOs;

public record ChangePasswordRequest(
    string PasswordOld,
    string PasswordNew
);

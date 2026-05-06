namespace MusicApp.Application.DTOs;

public record AuthResponse(
    string AccessToken,
    int ExpiresIn,
    UserDto User
);

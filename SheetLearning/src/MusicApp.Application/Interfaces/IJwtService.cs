using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}

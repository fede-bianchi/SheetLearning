using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Auth;

public class LogoutHandler
{
    private readonly ISessionRepository _sessionRepository;

    public LogoutHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<bool>> HandleAsync(string refreshToken)
    {
        var session = await _sessionRepository.GetByTokenAsync(refreshToken);
        if (session is not null)
        {
            await _sessionRepository.DeleteAsync(session);
        }

        return Result<bool>.Ok(true);
    }
}

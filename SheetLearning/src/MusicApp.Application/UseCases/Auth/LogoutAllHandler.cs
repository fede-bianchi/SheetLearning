using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Auth;

public class LogoutAllHandler
{
    private readonly ISessionRepository _sessionRepository;

    public LogoutAllHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<bool>> HandleAsync(int userId)
    {
        await _sessionRepository.DeleteAllForUserAsync(userId);
        return Result<bool>.Ok(true);
    }
}

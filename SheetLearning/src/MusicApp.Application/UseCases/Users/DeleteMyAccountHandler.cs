using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Users;

public class DeleteMyAccountHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ISessionRepository _sessionRepository;

    public DeleteMyAccountHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ISessionRepository sessionRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<bool>> HandleAsync(int userId, DeleteAccountRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result<bool>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");
        }

        if (!_passwordHasher.Verify(request.ConfermaPassword, user.PasswordHash))
        {
            return Result<bool>.Fail(ErrorCodes.WrongPassword, "Password di conferma non corretta.");
        }

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);
        await _sessionRepository.DeleteAllForUserAsync(userId);

        return Result<bool>.Ok(true);
    }
}

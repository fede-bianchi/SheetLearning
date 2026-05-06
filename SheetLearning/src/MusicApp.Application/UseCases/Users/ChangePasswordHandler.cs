using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Users;

public class ChangePasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> HandleAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result<bool>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");
        }

        if (!_passwordHasher.Verify(request.PasswordOld, user.PasswordHash))
        {
            return Result<bool>.Fail(ErrorCodes.WrongPassword, "Password corrente non corretta.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.PasswordNew);
        await _userRepository.UpdateAsync(user);

        return Result<bool>.Ok(true);
    }
}

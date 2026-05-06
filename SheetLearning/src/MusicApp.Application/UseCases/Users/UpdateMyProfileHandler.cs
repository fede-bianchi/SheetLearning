using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases;

namespace MusicApp.Application.UseCases.Users;

public class UpdateMyProfileHandler
{
    private readonly IUserRepository _userRepository;

    public UpdateMyProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserProfileDto>> HandleAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result<UserProfileDto>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");
        }

        if (!string.IsNullOrWhiteSpace(request.Nickname) &&
            !string.Equals(request.Nickname, user.Nickname, StringComparison.Ordinal))
        {
            var nicknameExists = await _userRepository.NicknameExistsAsync(request.Nickname);
            if (nicknameExists)
            {
                return Result<UserProfileDto>.Fail(ErrorCodes.NicknameConflict, "Nickname gia in uso.");
            }
        }

        if (request.Nome is not null)
        {
            user.Nome = request.Nome;
        }

        if (request.Cognome is not null)
        {
            user.Cognome = request.Cognome;
        }

        if (request.Nickname is not null)
        {
            user.Nickname = request.Nickname;
        }

        if (request.Descrizione is not null)
        {
            user.Descrizione = request.Descrizione;
        }

        if (request.Strumento is not null)
        {
            user.Strumento = request.Strumento;
        }

        var updatedUser = await _userRepository.UpdateAsync(user);
        return Result<UserProfileDto>.Ok(updatedUser.ToUserProfileDto());
    }
}

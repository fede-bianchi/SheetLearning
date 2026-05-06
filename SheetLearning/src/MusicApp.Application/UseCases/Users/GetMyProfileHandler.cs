using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases;

namespace MusicApp.Application.UseCases.Users;

public class GetMyProfileHandler
{
    private readonly IUserRepository _userRepository;

    public GetMyProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserProfileDto>> HandleAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result<UserProfileDto>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");
        }

        return Result<UserProfileDto>.Ok(user.ToUserProfileDto());
    }
}

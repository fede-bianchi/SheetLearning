using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.UseCases;

namespace MusicApp.Application.UseCases.Users;

public class GetPublicProfileHandler
{
    private readonly IUserRepository _userRepository;

    public GetPublicProfileHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<PublicUserDto>> HandleAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
        {
            return Result<PublicUserDto>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");
        }

        return Result<PublicUserDto>.Ok(user.ToPublicUserDto());
    }
}

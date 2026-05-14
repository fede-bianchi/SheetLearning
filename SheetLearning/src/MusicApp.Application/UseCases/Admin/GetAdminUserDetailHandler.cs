using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetAdminUserDetailHandler
{
    private readonly IUserRepository _userRepository;

    public GetAdminUserDetailHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AdminUserDto>> HandleAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithDetailsAsync(userId);
        if (user is null)
            return Result<AdminUserDto>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");

        return Result<AdminUserDto>.Ok(new AdminUserDto(
            user.Id,
            user.Nome,
            user.Cognome,
            user.Nickname,
            user.Email,
            user.Role.Nome,
            user.Plan.Nome,
            user.IsActive,
            user.Strumento,
            user.Descrizione,
            user.DataNascita,
            user.CreatedAt,
            user.UpdatedAt
        ));
    }
}

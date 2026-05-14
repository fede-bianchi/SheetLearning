using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Moderation;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Admin;

public class UnbanUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IModerationLogRepository _moderationLogRepository;

    public UnbanUserHandler(
        IUserRepository userRepository,
        IModerationLogRepository moderationLogRepository)
    {
        _userRepository = userRepository;
        _moderationLogRepository = moderationLogRepository;
    }

    public async Task<Result<bool>> HandleAsync(int adminId, int targetUserId)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId);
        if (user == null)
            return Result<bool>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");

        if (user.IsActive)
            return Result<bool>.Fail(ErrorCodes.UserNotBanned, "L'utente non è bannato.");

        user.IsActive = true;
        await _userRepository.UpdateAsync(user);

        await _moderationLogRepository.CreateAsync(new ModerationLog
        {
            AdminId        = adminId,
            TargetUserId   = targetUserId,
            Azione         = ModerationActions.UnbanUser,
            TargetType     = "user",
            TargetId       = targetUserId,
            Motivazione    = "Riattivazione account",
            ContenutoRimosso = null,
            CreatedAt      = DateTime.UtcNow
        });

        return Result<bool>.Ok(true);
    }
}

using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Moderation;
using MusicApp.Application.Notifications;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Admin;

public class WarnUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IModerationLogRepository _moderationLogRepository;
    private readonly INotificationService _notificationService;

    public WarnUserHandler(
        IUserRepository userRepository,
        IModerationLogRepository moderationLogRepository,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _moderationLogRepository = moderationLogRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> HandleAsync(int adminId, int targetUserId, string motivazione)
    {
        var user = await _userRepository.GetByIdAsync(targetUserId);
        if (user == null)
            return Result<bool>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");

        await _moderationLogRepository.CreateAsync(new ModerationLog
        {
            AdminId        = adminId,
            TargetUserId   = targetUserId,
            Azione         = ModerationActions.WarnUser,
            TargetType     = "user",
            TargetId       = targetUserId,
            Motivazione    = motivazione,
            ContenutoRimosso = null,
            CreatedAt      = DateTime.UtcNow
        });

        _ = _notificationService.SendAsync(
            recipientUserId : targetUserId,
            tipo            : NotificationTypes.ModerazioneRicevuta,
            titolo          : "Hai ricevuto un avviso formale",
            corpo           : $"Hai ricevuto un avviso formale dall'amministrazione. "
                            + $"Motivo: {motivazione}.",
            targetType      : "user",
            targetId        : targetUserId);

        return Result<bool>.Ok(true);
    }
}

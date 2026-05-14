using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Moderation;
using MusicApp.Application.Notifications;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Admin;

public class BanUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IModerationLogRepository _moderationLogRepository;
    private readonly INotificationService _notificationService;

    public BanUserHandler(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IModerationLogRepository moderationLogRepository,
        INotificationService notificationService)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _moderationLogRepository = moderationLogRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> HandleAsync(int adminId, int targetUserId, string motivazione)
    {
        if (adminId == targetUserId)
            return Result<bool>.Fail(ErrorCodes.AdminCannotTargetSelf, "Non puoi bannare te stesso.");

        var user = await _userRepository.GetByIdAsync(targetUserId);
        if (user == null)
            return Result<bool>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");

        if (!user.IsActive)
            return Result<bool>.Fail(ErrorCodes.UserAlreadyBanned, "L'utente è già bannato.");

        user.IsActive = false;
        await _userRepository.UpdateAsync(user);

        await _sessionRepository.DeleteAllForUserAsync(targetUserId);

        await _moderationLogRepository.CreateAsync(new ModerationLog
        {
            AdminId        = adminId,
            TargetUserId   = targetUserId,
            Azione         = ModerationActions.BanUser,
            TargetType     = "user",
            TargetId       = targetUserId,
            Motivazione    = motivazione,
            ContenutoRimosso = null,
            CreatedAt      = DateTime.UtcNow
        });

        _ = _notificationService.SendAsync(
            recipientUserId : targetUserId,
            tipo            : NotificationTypes.ModerazioneRicevuta,
            titolo          : "Il tuo account è stato sospeso",
            corpo           : $"Il tuo account è stato sospeso. Motivo: {motivazione}.",
            targetType      : "user",
            targetId        : targetUserId);

        return Result<bool>.Ok(true);
    }
}

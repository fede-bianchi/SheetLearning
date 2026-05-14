using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Moderation;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Admin;

public class ChangeUserRoleHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IModerationLogRepository _moderationLogRepository;
    private readonly ITeacherProfileRepository _teacherProfileRepository;

    public ChangeUserRoleHandler(
        IUserRepository userRepository,
        IModerationLogRepository moderationLogRepository,
        ITeacherProfileRepository teacherProfileRepository)
    {
        _userRepository = userRepository;
        _moderationLogRepository = moderationLogRepository;
        _teacherProfileRepository = teacherProfileRepository;
    }

    public async Task<Result<bool>> HandleAsync(int adminId, int targetUserId, string nuovoRuolo)
    {
        if (adminId == targetUserId)
            return Result<bool>.Fail(ErrorCodes.AdminCannotChangeOwnRole, "Non puoi cambiare il tuo ruolo.");

        var user = await _userRepository.GetByIdWithDetailsAsync(targetUserId);
        if (user == null)
            return Result<bool>.Fail(ErrorCodes.UserNotFound, "Utente non trovato.");

        int nuovoRoleId = nuovoRuolo switch
        {
            "Admin"       => 1,
            "Insegnante"  => 2,
            "Utente Pro"  => 3,
            "Utente"      => 4,
            _             => -1
        };

        if (nuovoRoleId == -1)
            return Result<bool>.Fail(ErrorCodes.InvalidRole, "Ruolo non valido.");

        var previousRuolo = user.Role.Nome;
        user.RoleId = nuovoRoleId;
        await _userRepository.UpdateAsync(user);

        if (nuovoRuolo == "Insegnante")
        {
            var existingProfile = await _teacherProfileRepository.GetByUserIdAsync(targetUserId);
            if (existingProfile == null)
            {
                await _teacherProfileRepository.CreateAsync(new TeacherProfile
                {
                    UserId           = targetUserId,
                    VisibileA        = "tutti",
                    Bio              = null,
                    Specializzazioni = null,
                    CreatedAt        = DateTime.UtcNow,
                    UpdatedAt        = DateTime.UtcNow
                });
            }
        }

        await _moderationLogRepository.CreateAsync(new ModerationLog
        {
            AdminId      = adminId,
            TargetUserId = targetUserId,
            Azione       = ModerationActions.ChangeRole,
            TargetType   = "user",
            TargetId     = targetUserId,
            Motivazione  = $"Ruolo cambiato da '{previousRuolo}' a '{nuovoRuolo}'.",
            CreatedAt    = DateTime.UtcNow
        });

        return Result<bool>.Ok(true);
    }
}

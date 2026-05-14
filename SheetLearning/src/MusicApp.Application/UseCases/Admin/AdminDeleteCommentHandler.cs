using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Moderation;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Admin;

public class AdminDeleteCommentHandler
{
    private readonly ICommentRepository _commentRepository;
    private readonly IModerationLogRepository _moderationLogRepository;
    private readonly INotificationService _notificationService;

    public AdminDeleteCommentHandler(
        ICommentRepository commentRepository,
        IModerationLogRepository moderationLogRepository,
        INotificationService notificationService)
    {
        _commentRepository = commentRepository;
        _moderationLogRepository = moderationLogRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> HandleAsync(int adminId, int commentId, string motivazione)
    {
        var comment = await _commentRepository.GetByIdAsync(commentId);
        if (comment == null)
            return Result<bool>.Fail(ErrorCodes.CommentNotFound, "Commento non trovato.");

        if (comment.IsDeleted)
            return Result<bool>.Ok(true);

        var contenutoCopied = comment.Contenuto;

        await _commentRepository.SoftDeleteAsync(comment);

        await _moderationLogRepository.CreateAsync(new ModerationLog
        {
            AdminId          = adminId,
            TargetUserId     = comment.UserId,
            Azione           = ModerationActions.DeleteComment,
            TargetType       = "comment",
            TargetId         = commentId,
            Motivazione      = motivazione,
            ContenutoRimosso = contenutoCopied,
            CreatedAt        = DateTime.UtcNow
        });

        _ = _notificationService.SendModerazioneRicevutaAsync(
            comment.UserId, "commento", motivazione);

        return Result<bool>.Ok(true);
    }
}

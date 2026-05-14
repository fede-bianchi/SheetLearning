using MusicApp.Application.Common;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Moderation;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Admin;

public class AdminDeletePostHandler
{
    private readonly IPostRepository _postRepository;
    private readonly IModerationLogRepository _moderationLogRepository;
    private readonly INotificationService _notificationService;

    public AdminDeletePostHandler(
        IPostRepository postRepository,
        IModerationLogRepository moderationLogRepository,
        INotificationService notificationService)
    {
        _postRepository = postRepository;
        _moderationLogRepository = moderationLogRepository;
        _notificationService = notificationService;
    }

    public async Task<Result<bool>> HandleAsync(int adminId, int postId, string motivazione)
    {
        var post = await _postRepository.GetByIdAsync(postId);
        if (post == null)
            return Result<bool>.Fail(ErrorCodes.PostNotFound, "Post non trovato.");

        if (post.IsDeleted)
            return Result<bool>.Ok(true);

        var contenutoCopied = post.Contenuto;

        await _postRepository.SoftDeleteAsync(post);

        await _moderationLogRepository.CreateAsync(new ModerationLog
        {
            AdminId          = adminId,
            TargetUserId     = post.UserId,
            Azione           = ModerationActions.DeletePost,
            TargetType       = "post",
            TargetId         = postId,
            Motivazione      = motivazione,
            ContenutoRimosso = contenutoCopied,
            CreatedAt        = DateTime.UtcNow
        });

        _ = _notificationService.SendModerazioneRicevutaAsync(
            post.UserId, "post", motivazione);

        return Result<bool>.Ok(true);
    }
}

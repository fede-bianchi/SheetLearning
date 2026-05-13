using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Notifications;

public class GetUnreadCountHandler
{
    private readonly INotificationRepository _notificationRepository;

    public GetUnreadCountHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<UnreadCountResponse>> HandleAsync(int userId)
    {
        var count = await _notificationRepository.GetUnreadCountAsync(userId);
        return Result<UnreadCountResponse>.Ok(new UnreadCountResponse(count));
    }
}

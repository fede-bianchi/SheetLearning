using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Notifications;

public class GetMyNotificationsHandler
{
    private readonly INotificationRepository _notificationRepository;

    public GetMyNotificationsHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Result<NotificationListResponse>> HandleAsync(int userId, NotificationQuery query)
    {
        var pageSize = Math.Min(query.PageSize, 50);

        var (items, totalCount) = await _notificationRepository.GetPagedAsync(userId, query.Page, pageSize);

        var dtos = items.Select(n => new NotificationDto(
            n.Id,
            n.Tipo,
            n.Titolo,
            n.Corpo,
            n.TargetType,
            n.TargetId,
            n.IsRead,
            n.IsArchived,
            n.CreatedAt,
            n.ReadAt
        )).ToList();

        return Result<NotificationListResponse>.Ok(new NotificationListResponse(
            dtos, totalCount, query.Page, pageSize));
    }
}

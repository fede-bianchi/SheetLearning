using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetModerationLogsHandler
{
    private readonly IModerationLogRepository _moderationLogRepository;

    public GetModerationLogsHandler(IModerationLogRepository moderationLogRepository)
    {
        _moderationLogRepository = moderationLogRepository;
    }

    public async Task<Result<ModerationLogListResponse>> HandleAsync(ModerationLogQuery query)
    {
        var pageSize = Math.Min(query.PageSize, 50);

        var al = query.Al ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var dal = query.Dal ?? al.AddDays(-30);

        var (items, totalCount) = await _moderationLogRepository.GetPagedAsync(
            query.FiltroAzione,
            query.FiltroAdminId,
            dal,
            al,
            query.Page,
            pageSize);

        var dtos = items.Select(l => new ModerationLogDto(
            l.Id,
            l.AdminId,
            l.Admin.Nickname,
            l.TargetUserId,
            l.TargetUser?.Nickname,
            l.Azione,
            l.TargetType,
            l.TargetId,
            l.Motivazione,
            l.ContenutoRimosso,
            l.CreatedAt
        )).ToList();

        return Result<ModerationLogListResponse>.Ok(new ModerationLogListResponse(
            dtos, totalCount, query.Page, pageSize));
    }
}

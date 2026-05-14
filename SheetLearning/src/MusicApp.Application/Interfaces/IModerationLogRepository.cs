using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IModerationLogRepository
{
    Task<ModerationLog> CreateAsync(ModerationLog log);

    Task<(IReadOnlyList<ModerationLog> Items, int TotalCount)> GetPagedAsync(
        string?  filtroAzione,
        int?     filtroAdminId,
        DateOnly? dal,
        DateOnly? al,
        int      page,
        int      pageSize);
}

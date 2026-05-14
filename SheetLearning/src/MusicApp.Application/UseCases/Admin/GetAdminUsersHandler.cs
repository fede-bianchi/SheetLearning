using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;

namespace MusicApp.Application.UseCases.Admin;

public class GetAdminUsersHandler
{
    private readonly IUserRepository _userRepository;

    public GetAdminUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<AdminUserListResponse>> HandleAsync(AdminUserQuery query)
    {
        var pageSize = Math.Min(query.PageSize, 50);

        var (users, totalCount) = await _userRepository.GetAdminPagedAsync(
            query.FiltroRuolo,
            query.FiltroAttivo,
            query.FiltroPiano,
            query.SearchQuery,
            query.Page,
            pageSize);

        var dtos = users.Select(u => new AdminUserDto(
            u.Id,
            u.Nome,
            u.Cognome,
            u.Nickname,
            u.Email,
            u.Role.Nome,
            u.Plan.Nome,
            u.IsActive,
            u.Strumento,
            u.Descrizione,
            u.DataNascita,
            u.CreatedAt,
            u.UpdatedAt
        )).ToList();

        return Result<AdminUserListResponse>.Ok(new AdminUserListResponse(
            dtos, totalCount, query.Page, pageSize));
    }
}

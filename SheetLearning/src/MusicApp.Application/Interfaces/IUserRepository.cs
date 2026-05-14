using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByNicknameAsync(string nickname);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> NicknameExistsAsync(string nickname);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);

    Task<(IReadOnlyList<User> Users, int TotalCount)> GetAdminPagedAsync(
        string? filtroRuolo,
        bool?   filtroAttivo,
        string? filtroPiano,
        string? searchQuery,
        int     page,
        int     pageSize);

    Task<User?> GetByIdWithDetailsAsync(int id);
}

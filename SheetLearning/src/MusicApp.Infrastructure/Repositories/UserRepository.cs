using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> GetByNicknameAsync(string nickname)
    {
        return await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Nickname == nickname);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _context.Users.AnyAsync(u => u.Email == email);
    }

    public Task<bool> NicknameExistsAsync(string nickname)
    {
        return _context.Users.AnyAsync(u => u.Nickname == nickname);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(user.Id) ?? user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(user.Id) ?? user;
    }

    public async Task<(IReadOnlyList<User> Users, int TotalCount)>
        GetAdminPagedAsync(
            string? filtroRuolo,
            bool?   filtroAttivo,
            string? filtroPiano,
            string? searchQuery,
            int     page,
            int     pageSize)
    {
        var query = _context.Users
            .Include(u => u.Role)
            .Include(u => u.Plan)
            .AsQueryable();

        if (filtroRuolo != null)
            query = query.Where(u => u.Role.Nome == filtroRuolo);
        if (filtroAttivo.HasValue)
            query = query.Where(u => u.IsActive == filtroAttivo.Value);
        if (filtroPiano != null)
            query = query.Where(u => u.Plan.Nome == filtroPiano);
        if (searchQuery != null)
            query = query.Where(u =>
                u.Nickname.Contains(searchQuery) ||
                u.Email.Contains(searchQuery));

        query = query.OrderByDescending(u => u.CreatedAt);

        var total = await query.CountAsync();
        var users = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (users, total);
    }

    public Task<User?> GetByIdWithDetailsAsync(int id)
        => _context.Users
            .Include(u => u.Role)
            .Include(u => u.Plan)
            .FirstOrDefaultAsync(u => u.Id == id);
}

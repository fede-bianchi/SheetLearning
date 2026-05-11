using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class TeacherProfileRepository : ITeacherProfileRepository
{
    private readonly AppDbContext _context;

    public TeacherProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<TeacherProfile>> GetVisibleAsync(
        string? planClaim, string? roleClaim, string? filtroCategoria)
    {
        var query = _context.TeacherProfiles
            .Include(tp => tp.User)
            .Include(tp => tp.Categories)
            .AsQueryable();

        bool isPro = planClaim == "Pro";
        bool isAdmin = roleClaim == "Admin";

        if (isAdmin)
        {
            query = query.Where(tp => tp.VisibileA != "nessuno");
        }
        else if (isPro)
        {
            query = query.Where(tp =>
                tp.VisibileA == "tutti" || tp.VisibileA == "solo_pro");
        }
        else
        {
            query = query.Where(tp => tp.VisibileA == "tutti");
        }

        if (filtroCategoria != null)
        {
            query = query.Where(tp =>
                tp.Categories.Any(c => c.Categoria == filtroCategoria));
        }

        return await query.AsNoTracking().ToListAsync();
    }

    public async Task<TeacherProfile?> GetByUserIdAsync(int userId)
    {
        return await _context.TeacherProfiles
            .Include(tp => tp.User)
            .Include(tp => tp.Categories)
            .FirstOrDefaultAsync(tp => tp.UserId == userId);
    }

    public async Task<TeacherProfile?> GetByIdAsync(int teacherProfileId)
    {
        return await _context.TeacherProfiles
            .Include(tp => tp.User)
            .Include(tp => tp.Categories)
            .FirstOrDefaultAsync(tp => tp.Id == teacherProfileId);
    }

    public async Task<TeacherProfile> CreateAsync(TeacherProfile profile)
    {
        _context.TeacherProfiles.Add(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task<TeacherProfile> UpdateAsync(TeacherProfile profile)
    {
        _context.TeacherProfiles.Update(profile);
        await _context.SaveChangesAsync();
        return profile;
    }

    public async Task SetCategoriesAsync(
        int teacherProfileId, IEnumerable<string> categories)
    {
        var existing = await _context.TeacherCategories
            .Where(tc => tc.TeacherId == teacherProfileId)
            .ToListAsync();
        _context.TeacherCategories.RemoveRange(existing);

        foreach (var cat in categories.Distinct())
        {
            _context.TeacherCategories.Add(new TeacherCategory
            {
                TeacherId = teacherProfileId,
                Categoria = cat
            });
        }

        await _context.SaveChangesAsync();
    }
}

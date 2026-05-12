using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Chat>> GetByUserAsync(int userId)
    {
        return await _context.Chats
            .Include(c => c.Student)
            .Include(c => c.Teacher)
            .Where(c => c.StudentId == userId || c.TeacherId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Chat?> GetByIdAsync(int id)
    {
        return await _context.Chats
            .Include(c => c.Student)
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Chat?> GetByParticipantsAsync(int studentId, int teacherId)
    {
        return await _context.Chats
            .FirstOrDefaultAsync(c =>
                c.StudentId == studentId && c.TeacherId == teacherId);
    }

    public async Task<Chat> CreateAsync(Chat chat)
    {
        _context.Chats.Add(chat);
        await _context.SaveChangesAsync();
        return chat;
    }

    public async Task UpdateLastMessageAtAsync(int chatId, DateTime lastMessageAt)
    {
        await _context.Chats
            .Where(c => c.Id == chatId)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(c => c.LastMessageAt, lastMessageAt));
    }
}

using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Repositories;

public class ChatMessageRepository : IChatMessageRepository
{
    private readonly AppDbContext _context;

    public ChatMessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<ChatMessage> Messages, int TotalCount)>
        GetPagedAsync(int chatId, int page, int pageSize)
    {
        var query = _context.ChatMessages
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt);

        var total = await query.CountAsync();
        var messages = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

        return (messages, total);
    }

    public async Task<ChatMessage> CreateAsync(ChatMessage message)
    {
        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task<ChatMessage?> GetLastMessageAsync(int chatId)
    {
        var msg = await _context.ChatMessages
            .Where(m => m.ChatId == chatId)
            .OrderByDescending(m => m.CreatedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        if (msg == null) return null;

        msg.Contenuto = msg.Contenuto.Length > 100
            ? msg.Contenuto[..100]
            : msg.Contenuto;
        return msg;
    }

    public async Task<int> GetUnreadCountAsync(int chatId, int excludeUserId)
    {
        return await _context.ChatMessages
            .CountAsync(m =>
                m.ChatId == chatId &&
                m.SenderId != excludeUserId &&
                !m.Letto);
    }

    public async Task<int> MarkAllReadAsync(int chatId, int excludeUserId)
    {
        return await _context.ChatMessages
            .Where(m =>
                m.ChatId == chatId &&
                m.SenderId != excludeUserId &&
                !m.Letto)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(m => m.Letto, true));
    }
}

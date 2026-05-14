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

    public async Task<Dictionary<int, int>> GetUnreadCountBatchAsync(
        IEnumerable<int> chatIds, int excludeUserId)
    {
        var ids = chatIds.ToList();
        var counts = await _context.ChatMessages
            .Where(m => ids.Contains(m.ChatId)
                     && m.SenderId != excludeUserId
                     && !m.Letto)
            .GroupBy(m => m.ChatId)
            .Select(g => new { ChatId = g.Key, Count = g.Count() })
            .ToListAsync();

        return ids.ToDictionary(
            id => id,
            id => counts.FirstOrDefault(c => c.ChatId == id)?.Count ?? 0);
    }

    public async Task<Dictionary<int, string?>> GetLastMessageBatchAsync(
        IEnumerable<int> chatIds)
    {
        var ids = chatIds.ToList();

        var latestIds = await _context.ChatMessages
            .Where(m => ids.Contains(m.ChatId))
            .GroupBy(m => m.ChatId)
            .Select(g => g.OrderByDescending(m => m.CreatedAt).First().Id)
            .ToListAsync();

        var messages = await _context.ChatMessages
            .Where(m => latestIds.Contains(m.Id))
            .Select(m => new { m.ChatId, m.Contenuto })
            .ToListAsync();

        return ids.ToDictionary(
            id => id,
            id => messages.FirstOrDefault(m => m.ChatId == id) is { } msg
                  ? (msg.Contenuto.Length > 100
                      ? msg.Contenuto[..100]
                      : msg.Contenuto)
                  : null);
    }
}

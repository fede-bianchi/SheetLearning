using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IChatMessageRepository
{
    Task<(IReadOnlyList<ChatMessage> Messages, int TotalCount)> GetPagedAsync(
        int chatId, int page, int pageSize);
    Task<ChatMessage> CreateAsync(ChatMessage message);
    Task<ChatMessage?> GetLastMessageAsync(int chatId);
    Task<int> GetUnreadCountAsync(int chatId, int excludeUserId);
    Task<int> MarkAllReadAsync(int chatId, int excludeUserId);
}

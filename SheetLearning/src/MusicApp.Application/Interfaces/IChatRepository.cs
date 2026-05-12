using MusicApp.Domain.Entities;

namespace MusicApp.Application.Interfaces;

public interface IChatRepository
{
    Task<IReadOnlyList<Chat>> GetByUserAsync(int userId);
    Task<Chat?> GetByIdAsync(int id);
    Task<Chat?> GetByParticipantsAsync(int studentId, int teacherId);
    Task<Chat> CreateAsync(Chat chat);
    Task UpdateLastMessageAtAsync(int chatId, DateTime lastMessageAt);
}

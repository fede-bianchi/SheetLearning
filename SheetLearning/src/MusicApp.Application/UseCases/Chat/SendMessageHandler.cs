using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Chat;

public class SendMessageHandler
{
    private readonly IChatRepository _chatRepo;
    private readonly IChatMessageRepository _messageRepo;
    private readonly IUserRepository _userRepo;
    private readonly IChatNotificationService _notificationService;

    public SendMessageHandler(
        IChatRepository chatRepo,
        IChatMessageRepository messageRepo,
        IUserRepository userRepo,
        IChatNotificationService notificationService)
    {
        _chatRepo = chatRepo;
        _messageRepo = messageRepo;
        _userRepo = userRepo;
        _notificationService = notificationService;
    }

    public async Task<Result<ChatMessageDto>> HandleAsync(
        int currentUserId, int teacherId, SendMessageRequest request)
    {
        if (currentUserId == teacherId)
            return Result<ChatMessageDto>.Fail(
                ErrorCodes.CannotMessageSelf,
                "Non puoi inviare un messaggio a te stesso.");

        var target = await _userRepo.GetByIdAsync(teacherId);
        if (target == null)
            return Result<ChatMessageDto>.Fail(
                ErrorCodes.TeacherNotFound, "Destinatario non trovato.");

        var chat = await _chatRepo.GetByParticipantsAsync(
            currentUserId, teacherId);
        if (chat == null)
        {
            chat = await _chatRepo.CreateAsync(new Domain.Entities.Chat
            {
                StudentId = currentUserId,
                TeacherId = teacherId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            });
        }

        var message = await _messageRepo.CreateAsync(new ChatMessage
        {
            ChatId = chat.Id,
            SenderId = currentUserId,
            Contenuto = request.Contenuto,
            Letto = false,
            CreatedAt = DateTime.UtcNow
        });

        await _chatRepo.UpdateLastMessageAtAsync(chat.Id, DateTime.UtcNow);

        int recipientId = (currentUserId == chat.StudentId)
            ? chat.TeacherId
            : chat.StudentId;

        var sender = await _userRepo.GetByIdAsync(currentUserId);

        var dto = new ChatMessageDto(
            Id: message.Id,
            ChatId: chat.Id,
            SenderId: currentUserId,
            SenderNickname: sender!.Nickname,
            Contenuto: message.Contenuto,
            Letto: false,
            CreatedAt: message.CreatedAt
        );

        _ = _notificationService.NotifyNewMessageAsync(recipientId, dto);

        return Result<ChatMessageDto>.Ok(dto);
    }
}

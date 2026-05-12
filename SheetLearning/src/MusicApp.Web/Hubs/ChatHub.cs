using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MusicApp.Application.Interfaces;

namespace MusicApp.Web.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatRepository _chatRepo;

    public ChatHub(IChatRepository chatRepo)
        => _chatRepo = chatRepo;

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        if (userId.HasValue)
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"user_{userId.Value}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChat(int chatId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var chat = await _chatRepo.GetByIdAsync(chatId);
        if (chat == null) return;
        if (chat.StudentId != userId.Value && chat.TeacherId != userId.Value)
            return;

        await Groups.AddToGroupAsync(
            Context.ConnectionId, $"chat_{chatId}");
    }

    public async Task LeaveChat(int chatId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId, $"chat_{chatId}");
    }

    public async Task StartTyping(int chatId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var nickname = Context.User?.FindFirst("nickname")?.Value ?? "Unknown";

        await Clients
            .GroupExcept($"chat_{chatId}", Context.ConnectionId)
            .SendAsync("UserTyping", chatId, nickname);
    }

    public async Task StopTyping(int chatId)
    {
        var userId = GetUserId();
        if (userId == null) return;

        var nickname = Context.User?.FindFirst("nickname")?.Value ?? "Unknown";

        await Clients
            .GroupExcept($"chat_{chatId}", Context.ConnectionId)
            .SendAsync("UserStoppedTyping", chatId, nickname);
    }

    private int? GetUserId()
    {
        var sub = Context.User?.FindFirst("sub")?.Value;
        return int.TryParse(sub, out var id) ? id : null;
    }
}

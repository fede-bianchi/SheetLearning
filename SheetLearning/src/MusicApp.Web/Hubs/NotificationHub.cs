using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace MusicApp.Web.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var sub = Context.User?.FindFirst("sub")?.Value;
        if (sub != null && int.TryParse(sub, out var userId))
            await Groups.AddToGroupAsync(
                Context.ConnectionId, $"notif_user_{userId}");

        await base.OnConnectedAsync();
    }
}

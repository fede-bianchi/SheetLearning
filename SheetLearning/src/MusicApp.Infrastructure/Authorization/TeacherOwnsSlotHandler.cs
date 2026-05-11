using Microsoft.AspNetCore.Authorization;
using MusicApp.Application.Authorization;

namespace MusicApp.Infrastructure.Authorization;

public class TeacherOwnsSlotHandler
    : AuthorizationHandler<TeacherOwnsSlotRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TeacherOwnsSlotRequirement requirement,
        IOwnedResource resource)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        if (sub != null && int.TryParse(sub, out var userId)
            && userId == resource.OwnerId)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

using Microsoft.AspNetCore.Authorization;
using MusicApp.Application.Authorization;

namespace MusicApp.Infrastructure.Authorization;

public class OwnerOrAdminHandler
    : AuthorizationHandler<OwnerOrAdminRequirement, IOwnedResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OwnerOrAdminRequirement requirement,
        IOwnedResource resource)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        var role = context.User.FindFirst("role")?.Value;

        if (role == "Admin")
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (sub != null && int.TryParse(sub, out var userId) && userId == resource.OwnerId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

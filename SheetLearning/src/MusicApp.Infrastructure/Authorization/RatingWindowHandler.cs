using Microsoft.AspNetCore.Authorization;
using MusicApp.Application.Authorization;

namespace MusicApp.Infrastructure.Authorization;

public class RatingWindowHandler
    : AuthorizationHandler<RatingWindowRequirement, RatingWindowResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        RatingWindowRequirement requirement,
        RatingWindowResource resource)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        if (sub == null || !int.TryParse(sub, out var userId))
            return Task.CompletedTask;

        if (userId != resource.StudentId)
            return Task.CompletedTask;

        if (resource.Stato != "completata")
            return Task.CompletedTask;

        if (DateTime.UtcNow <= resource.UpdatedAt.AddHours(48))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

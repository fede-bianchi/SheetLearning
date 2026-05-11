using Microsoft.AspNetCore.Authorization;
using MusicApp.Application.Authorization;

namespace MusicApp.Infrastructure.Authorization;

public class BookingParticipantHandler
    : AuthorizationHandler<BookingParticipantRequirement,
                           BookingParticipantResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BookingParticipantRequirement requirement,
        BookingParticipantResource resource)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        if (sub != null && int.TryParse(sub, out var userId)
            && (userId == resource.TeacherId || userId == resource.StudentId))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}

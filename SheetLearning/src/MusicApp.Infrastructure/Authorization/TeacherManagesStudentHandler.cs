using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using MusicApp.Application.Authorization;
using MusicApp.Infrastructure.Persistence;

namespace MusicApp.Infrastructure.Authorization;

public class TeacherManagesStudentHandler
    : AuthorizationHandler<TeacherManagesStudentRequirement,
                           TeacherStudentResource>
{
    private readonly AppDbContext _context;

    public TeacherManagesStudentHandler(AppDbContext context)
        => _context = context;

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TeacherManagesStudentRequirement requirement,
        TeacherStudentResource resource)
    {
        var sub = context.User.FindFirst("sub")?.Value;
        if (sub == null || !int.TryParse(sub, out var userId))
            return;

        if (userId != resource.TeacherId)
            return;

        var hasRelationship = await _context.LessonBookings
            .AnyAsync(b =>
                b.TeacherId == resource.TeacherId &&
                b.StudentId == resource.StudentId &&
                (b.Stato == "confermata" || b.Stato == "completata"));

        if (hasRelationship)
            context.Succeed(requirement);
    }
}

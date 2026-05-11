using Microsoft.AspNetCore.Authorization;

namespace MusicApp.Application.Authorization;

public class TeacherOwnsBookingRequirement : IAuthorizationRequirement
{
}

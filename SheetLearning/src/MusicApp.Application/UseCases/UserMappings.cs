using MusicApp.Application.DTOs;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases;

internal static class UserMappings
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.Nome,
            user.Cognome,
            user.Nickname,
            user.Email,
            user.Role?.Nome ?? ResolveRoleName(user.RoleId),
            user.Plan?.Nome ?? ResolvePlanName(user.PlanId),
            user.IsActive,
            user.Strumento
        );
    }

    public static UserProfileDto ToUserProfileDto(this User user)
    {
        return new UserProfileDto(
            user.Id,
            user.Nome,
            user.Cognome,
            user.Nickname,
            user.Email,
            user.Role?.Nome ?? ResolveRoleName(user.RoleId),
            user.Plan?.Nome ?? ResolvePlanName(user.PlanId),
            user.IsActive,
            user.Strumento,
            user.Descrizione,
            user.DataNascita,
            user.CreatedAt
        );
    }

    public static PublicUserDto ToPublicUserDto(this User user)
    {
        return new PublicUserDto(
            user.Id,
            user.Nickname,
            user.Strumento,
            user.Descrizione
        );
    }

    private static string ResolveRoleName(int roleId)
    {
        return roleId switch
        {
            1 => "Admin",
            2 => "Insegnante",
            3 => "Utente Pro",
            _ => "Utente"
        };
    }

    private static string ResolvePlanName(int planId)
    {
        return planId == 2 ? "Pro" : "Standard";
    }
}

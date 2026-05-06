using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MusicApp.Application.UseCases.Auth;
using MusicApp.Application.UseCases.Users;
using MusicApp.Application.Validators;

namespace MusicApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        services.AddScoped<RegisterUserHandler>();
        services.AddScoped<LoginUserHandler>();
        services.AddScoped<RefreshTokenHandler>();
        services.AddScoped<LogoutHandler>();
        services.AddScoped<LogoutAllHandler>();

        services.AddScoped<GetMyProfileHandler>();
        services.AddScoped<UpdateMyProfileHandler>();
        services.AddScoped<ChangePasswordHandler>();
        services.AddScoped<DeleteMyAccountHandler>();
        services.AddScoped<GetPublicProfileHandler>();

        return services;
    }
}

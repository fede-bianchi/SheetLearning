using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MusicApp.Application.UseCases.Auth;
using MusicApp.Application.UseCases.Exercises;
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

        services.AddScoped<GetExerciseTypesHandler>();
        services.AddScoped<GetLevelsHandler>();
        services.AddScoped<GetLevelDetailHandler>();
        services.AddScoped<GetLevelsByExerciseTypeHandler>();
        services.AddScoped<GetMyProgressHandler>();
        services.AddScoped<SaveAttemptHandler>();
        services.AddScoped<GetMyBestScoresHandler>();
        services.AddScoped<GetMyAttemptsHandler>();

        return services;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MusicApp.Application.Interfaces;
using MusicApp.Infrastructure.Persistence;
using MusicApp.Infrastructure.Repositories;
using MusicApp.Infrastructure.Security;
using MusicApp.Infrastructure.Services;

namespace MusicApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        services.AddScoped<IExerciseTypeRepository, ExerciseTypeRepository>();
        services.AddScoped<ILevelRepository, LevelRepository>();
        services.AddScoped<IAttemptRepository, AttemptRepository>();
        services.AddScoped<IBestScoreRepository, BestScoreRepository>();
        services.AddScoped<IUserLevelProgressRepository, UserLevelProgressRepository>();
        services.AddScoped<IPostRepository, PostRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Phase 4
        services.AddScoped<ITeacherProfileRepository, TeacherProfileRepository>();
        services.AddScoped<ILessonSlotRepository, LessonSlotRepository>();
        services.AddScoped<ILessonBookingRepository, LessonBookingRepository>();
        services.AddScoped<ILessonBundleRepository, LessonBundleRepository>();
        services.AddScoped<ILessonBundlePurchaseRepository, LessonBundlePurchaseRepository>();
        services.AddScoped<ILessonRatingRepository, LessonRatingRepository>();

        // Phase 5
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IStripeService, StripeService>();

        // Phase 6
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();

        // Phase 7
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationService, NotificationService>();

        // Phase 8
        services.AddScoped<IModerationLogRepository, ModerationLogRepository>();
        services.AddScoped<IAdminStatsRepository, AdminStatsRepository>();

        return services;
    }
}

using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MusicApp.Application.UseCases.Auth;
using MusicApp.Application.UseCases.Bookings;
using MusicApp.Application.UseCases.Bundles;
using MusicApp.Application.UseCases.Exercises;
using MusicApp.Application.UseCases.Forum;
using MusicApp.Application.UseCases.Payments;
using MusicApp.Application.UseCases.Slots;
using MusicApp.Application.UseCases.Teachers;
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

        services.AddScoped<GetPostsHandler>();
        services.AddScoped<GetPostDetailHandler>();
        services.AddScoped<CreatePostHandler>();
        services.AddScoped<UpdatePostHandler>();
        services.AddScoped<DeletePostHandler>();
        services.AddScoped<GetCommentsHandler>();
        services.AddScoped<CreateCommentHandler>();
        services.AddScoped<UpdateCommentHandler>();
        services.AddScoped<DeleteCommentHandler>();
        services.AddScoped<UpsertVoteHandler>();
        services.AddScoped<RemoveVoteHandler>();

        // Phase 4 — Teachers
        services.AddScoped<GetTeachersHandler>();
        services.AddScoped<GetTeacherPublicProfileHandler>();
        services.AddScoped<GetMyTeacherProfileHandler>();
        services.AddScoped<UpdateMyTeacherProfileHandler>();
        services.AddScoped<SetMyCategoriesHandler>();
        services.AddScoped<GetMyStudentsHandler>();
        services.AddScoped<GetStudentDetailHandler>();
        services.AddScoped<GetStudentBookingsHandler>();
        // Phase 4 — Slots
        services.AddScoped<GetTeacherSlotsHandler>();
        services.AddScoped<GetMyTeacherSlotsHandler>();
        services.AddScoped<CreateSlotHandler>();
        services.AddScoped<DeleteSlotHandler>();
        // Phase 4 — Bookings
        services.AddScoped<GetMyBookingsHandler>();
        services.AddScoped<GetBookingDetailHandler>();
        services.AddScoped<CreateBookingHandler>();
        services.AddScoped<ConfirmBookingHandler>();
        services.AddScoped<CancelBookingHandler>();
        services.AddScoped<CompleteBookingHandler>();
        services.AddScoped<CreateRatingHandler>();
        // Phase 4 — Bundles
        services.AddScoped<GetActiveBundlesHandler>();
        services.AddScoped<GetMyBundlePurchasesHandler>();

        // Phase 5 — Payments
        services.AddScoped<CreateProCheckoutHandler>();
        services.AddScoped<CreateBundleCheckoutHandler>();
        services.AddScoped<HandleStripeWebhookHandler>();
        services.AddScoped<GetMySubscriptionHandler>();
        services.AddScoped<GetMyPaymentsHandler>();
        services.AddScoped<CancelSubscriptionHandler>();

        return services;
    }
}

using System.Threading.RateLimiting;
using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using MusicApp.Application;
using MusicApp.Application.Authorization;
using MusicApp.Application.Caching;
using MusicApp.Application.Interfaces;
using MusicApp.Infrastructure;
using MusicApp.Infrastructure.Authorization;
using MusicApp.Infrastructure.BackgroundJobs;
using MusicApp.Infrastructure.Caching;
using MusicApp.Infrastructure.Persistence;
using MusicApp.Web.Infrastructure;
using MusicApp.Web.Middleware;
using MusicApp.Web.Models;
using MusicApp.Web.Options;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using StackExchange.Redis;
using Stripe;
using ApplicationJwtOptions = MusicApp.Application.Options.JwtOptions;
using ApplicationPlansOptions = MusicApp.Application.Options.PlansOptions;
using ApplicationStripeOptions = MusicApp.Application.Options.StripeOptions;
using WebJwtOptions = MusicApp.Web.Options.JwtOptions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Server=localhost;Port=3306;Database=musicapp;User=musicapp;Password=changeme;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 0, 0))));

builder.Services.Configure<ApplicationJwtOptions>(
    builder.Configuration.GetSection(ApplicationJwtOptions.SectionName));

builder.Services.Configure<WebJwtOptions>(
    builder.Configuration.GetSection(WebJwtOptions.SectionName));

// Phase 5 — Stripe and Plans options
builder.Services.Configure<ApplicationStripeOptions>(
    builder.Configuration.GetSection(ApplicationStripeOptions.SectionName));
builder.Services.Configure<ApplicationPlansOptions>(
    builder.Configuration.GetSection(ApplicationPlansOptions.SectionName));

// Phase 5 — Stripe SDK initialization
var stripeConfig = builder.Configuration
    .GetSection(ApplicationStripeOptions.SectionName)
    .Get<ApplicationStripeOptions>()!;
StripeConfiguration.ApiKey = stripeConfig.SecretKey;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IRefreshCookieHelper, RefreshCookieHelper>();
builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrAdminHandler>();

// Phase 6 — Chat notification service (Web project, needs SignalR)
builder.Services.AddScoped<MusicApp.Application.Interfaces.IChatNotificationService,
                            MusicApp.Web.Services.SignalRChatNotificationService>();

// Phase 7 — Notification push service (Web project, needs SignalR)
builder.Services.AddScoped<INotificationPushService,
                            MusicApp.Web.Services.SignalRNotificationPushService>();

// Phase 9 — Background jobs (replaces TODO from Phase 7)
builder.Services.AddHostedService<SubscriptionExpiryJob>();
builder.Services.AddHostedService<BundleExpiryJob>();

// Phase 4 — authorization handler registrations
builder.Services.AddSingleton<IAuthorizationHandler, TeacherOwnsSlotHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, TeacherOwnsBookingHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, StudentOwnsBookingHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, BookingParticipantHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TeacherManagesStudentHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, RatingWindowHandler>();

var jwtOptions = builder.Configuration.GetSection(WebJwtOptions.SectionName)
    .Get<WebJwtOptions>() ?? new WebJwtOptions();

if (string.IsNullOrWhiteSpace(jwtOptions.Secret))
{
    jwtOptions.Secret = "replace_with_32_char_minimum_secret";
}

builder.Services.PostConfigure<ApplicationJwtOptions>(options =>
{
    if (string.IsNullOrWhiteSpace(options.Secret))
    {
        options.Secret = jwtOptions.Secret;
    }

    if (options.AccessTokenExpiryMinutes <= 0)
    {
        options.AccessTokenExpiryMinutes = 15;
    }

    if (options.RefreshTokenExpiryDays <= 0)
    {
        options.RefreshTokenExpiryDays = 30;
    }
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ClockSkew = TimeSpan.Zero
        };

        // Phase 6 — Accept JWT from query string for SignalR
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken)
                    && (path.StartsWithSegments("/hubs/chat")
                     || path.StartsWithSegments("/hubs/notifications")))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .RequireClaim("is_active", "True")
        .Build();

    options.AddPolicy("CanDeleteOwnAccount", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("is_active", "True")
            .RequireAssertion(ctx =>
                !ctx.User.HasClaim("role", "Admin")));

    options.AddPolicy("OwnerOrAdmin", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("is_active", "True")
            .AddRequirements(new OwnerOrAdminRequirement()));

    // Phase 4 — IsTeacher policy (claim-based, no resource handler)
    options.AddPolicy("IsTeacher", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .RequireClaim("role", "Insegnante"));

    // Phase 4 — resource-based policies
    options.AddPolicy("TeacherOwnsSlot", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .RequireClaim("role", "Insegnante")
              .AddRequirements(new TeacherOwnsSlotRequirement()));

    options.AddPolicy("TeacherOwnsBooking", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .RequireClaim("role", "Insegnante")
              .AddRequirements(new TeacherOwnsBookingRequirement()));

    options.AddPolicy("StudentOwnsBooking", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .AddRequirements(new StudentOwnsBookingRequirement()));

    options.AddPolicy("BookingParticipant", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .AddRequirements(new BookingParticipantRequirement()));

    options.AddPolicy("TeacherManagesStudent", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .RequireClaim("role", "Insegnante")
              .AddRequirements(new TeacherManagesStudentRequirement()));

    options.AddPolicy("RatingWindow", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .AddRequirements(new RatingWindowRequirement()));

    // Phase 8 — IsAdmin
    options.AddPolicy("IsAdmin", policy =>
        policy.RequireAuthenticatedUser()
              .RequireClaim("is_active", "True")
              .RequireClaim("role", "Admin"));
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// Phase 6 — SignalR
var redisConnection = builder.Configuration["Redis:ConnectionString"];

if (!string.IsNullOrEmpty(redisConnection))
{
    builder.Services.AddSignalR()
        .AddStackExchangeRedis(redisConnection, options =>
        {
            options.Configuration.ChannelPrefix =
                RedisChannel.Literal("MusicApp");
        });
}
else
{
    builder.Services.AddSignalR();
}

// Phase 6 — CORS for SignalR WebSocket
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Phase 9 — Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit           = 10;
        limiterOptions.Window                = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder  = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit            = 0;
    });

    options.AddFixedWindowLimiter("GeneralPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window      = TimeSpan.FromMinutes(1);
        limiterOptions.QueueLimit  = 0;
    });

    options.OnRejected = async (context, _) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsJsonAsync(
            new ApiError("RATE_LIMIT_EXCEEDED",
                "Too many requests. Please try again in a moment."));
    };
});

// Phase 9 — Caching
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// Phase 9 — Global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Phase 9 — Health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

var app = builder.Build();

// Phase 9 — Global exception handler (must be first)
app.UseExceptionHandler();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var sessionRepository = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
    await sessionRepository.DeleteExpiredAsync();
}

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(context);
}

app.UseHttpsRedirection();

// Phase 6 — CORS for SignalR WebSocket
app.UseCors();

// Phase 5 — Enable request body buffering for Stripe webhook raw body reading
app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

// Phase 9 — Rate limiting (after routing, before auth)
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Phase 6 — SignalR hub endpoint
app.MapHub<MusicApp.Web.Hubs.ChatHub>("/hubs/chat");

// Phase 7 — Notification hub
app.MapHub<MusicApp.Web.Hubs.NotificationHub>("/hubs/notifications");

// Phase 9 — Health check endpoint
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new
        {
            status   = report.Status.ToString(),
            duration = report.TotalDuration.TotalMilliseconds,
            checks   = report.Entries.Select(e => new
            {
                name     = e.Key,
                status   = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                error    = e.Value.Exception?.Message
            })
        });
    }
});

app.Run();

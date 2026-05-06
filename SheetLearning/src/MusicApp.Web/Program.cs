using System.Text;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MusicApp.Application;
using MusicApp.Application.Interfaces;
using MusicApp.Infrastructure;
using MusicApp.Infrastructure.Persistence;
using MusicApp.Web.Infrastructure;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ApplicationJwtOptions = MusicApp.Application.Options.JwtOptions;
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

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IRefreshCookieHelper, RefreshCookieHelper>();

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
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

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

app.MapControllers();
app.MapRazorPages();

app.Run();

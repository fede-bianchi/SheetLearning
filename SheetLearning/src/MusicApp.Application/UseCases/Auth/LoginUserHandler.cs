using Microsoft.Extensions.Options;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Options;
using MusicApp.Application.UseCases;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Auth;

public class LoginUserHandler
{
    private const string DummyArgonHash = "$argon2id$v=19$m=65536,t=3,p=4$dummysalt$dummyhash";
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ISessionRepository _sessionRepository;
    private readonly JwtOptions _jwtOptions;

    public LoginUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        ISessionRepository sessionRepository,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _sessionRepository = sessionRepository;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>> HandleAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user is null)
        {
            _passwordHasher.Verify(request.Password, DummyArgonHash);

            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.InvalidCredentials,
                "Email o password non validi.");
        }

        if (!user.IsActive)
        {
            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.AccountInactive,
                "Account disattivato.");
        }

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.InvalidCredentials,
                "Email o password non validi.");
        }

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        await _sessionRepository.CreateAsync(new Session
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = refreshTokenExpiry
        });

        var authResponse = new AuthResponse(
            accessToken,
            AccessTokenExpiresInSeconds,
            user.ToUserDto());

        return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Ok(
            (authResponse, refreshToken, refreshTokenExpiry));
    }
}

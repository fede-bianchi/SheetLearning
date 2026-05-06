using Microsoft.Extensions.Options;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Options;
using MusicApp.Application.UseCases;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Auth;

public class RefreshTokenHandler
{
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly JwtOptions _jwtOptions;

    public RefreshTokenHandler(
        ISessionRepository sessionRepository,
        IUserRepository userRepository,
        IJwtService jwtService,
        IOptions<JwtOptions> jwtOptions)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _jwtService = jwtService;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>> HandleAsync(string refreshToken)
    {
        var existingSession = await _sessionRepository.GetByTokenAsync(refreshToken);
        if (existingSession is null)
        {
            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.InvalidRefreshToken,
                "Refresh token non valido.");
        }

        if (existingSession.ExpiresAt <= DateTime.UtcNow)
        {
            await _sessionRepository.DeleteAsync(existingSession);

            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.ExpiredRefreshToken,
                "Refresh token scaduto.");
        }

        var user = await _userRepository.GetByIdAsync(existingSession.UserId);
        if (user is null)
        {
            await _sessionRepository.DeleteAsync(existingSession);

            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.InvalidRefreshToken,
                "Refresh token non valido.");
        }

        if (!user.IsActive)
        {
            await _sessionRepository.DeleteAsync(existingSession);

            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.AccountInactive,
                "Account disattivato.");
        }

        await _sessionRepository.DeleteAsync(existingSession);

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        await _sessionRepository.CreateAsync(new Session
        {
            UserId = user.Id,
            Token = newRefreshToken,
            ExpiresAt = refreshTokenExpiry
        });

        var authResponse = new AuthResponse(
            newAccessToken,
            AccessTokenExpiresInSeconds,
            user.ToUserDto());

        return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Ok(
            (authResponse, newRefreshToken, refreshTokenExpiry));
    }
}

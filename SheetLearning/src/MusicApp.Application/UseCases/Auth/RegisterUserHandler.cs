using Microsoft.Extensions.Options;
using MusicApp.Application.Common;
using MusicApp.Application.DTOs;
using MusicApp.Application.Interfaces;
using MusicApp.Application.Options;
using MusicApp.Application.UseCases;
using MusicApp.Domain.Entities;

namespace MusicApp.Application.UseCases.Auth;

public class RegisterUserHandler
{
    private const int AccessTokenExpiresInSeconds = 900;

    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ISessionRepository _sessionRepository;
    private readonly JwtOptions _jwtOptions;

    public RegisterUserHandler(
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

    public async Task<Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>> HandleAsync(RegisterRequest request)
    {
        if (await _userRepository.EmailExistsAsync(request.Email))
        {
            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.EmailAlreadyExists,
                "Email gia registrata.");
        }

        if (await _userRepository.NicknameExistsAsync(request.Nickname))
        {
            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.NicknameAlreadyExists,
                "Nickname gia in uso.");
        }

        if (CalculateAge(request.DataNascita, DateOnly.FromDateTime(DateTime.UtcNow)) < 18)
        {
            return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Fail(
                ErrorCodes.UnderAge,
                "Devi avere almeno 18 anni per registrarti.");
        }

        var user = new User
        {
            Nome = request.Nome,
            Cognome = request.Cognome,
            Nickname = request.Nickname,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DataNascita = request.DataNascita,
            Strumento = request.Strumento,
            RoleId = 4,
            PlanId = 1,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);
        var accessToken = _jwtService.GenerateAccessToken(createdUser);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays);

        await _sessionRepository.CreateAsync(new Session
        {
            UserId = createdUser.Id,
            Token = refreshToken,
            ExpiresAt = refreshTokenExpiry
        });

        var authResponse = new AuthResponse(
            accessToken,
            AccessTokenExpiresInSeconds,
            createdUser.ToUserDto());

        return Result<(AuthResponse Response, string RefreshToken, DateTime RefreshTokenExpiry)>.Ok(
            (authResponse, refreshToken, refreshTokenExpiry));
    }

    private static int CalculateAge(DateOnly birthDate, DateOnly referenceDate)
    {
        var age = referenceDate.Year - birthDate.Year;
        var hadBirthday =
            referenceDate.Month > birthDate.Month ||
            (referenceDate.Month == birthDate.Month && referenceDate.Day >= birthDate.Day);

        if (!hadBirthday)
        {
            age--;
        }

        return age;
    }
}

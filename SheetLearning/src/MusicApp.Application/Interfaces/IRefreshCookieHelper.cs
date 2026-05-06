using Microsoft.AspNetCore.Http;

namespace MusicApp.Application.Interfaces;

public interface IRefreshCookieHelper
{
    void SetRefreshCookie(HttpResponse response, string token, DateTime expiresAt);
    void ClearRefreshCookie(HttpResponse response);
    string? ReadRefreshToken(HttpRequest request);
}

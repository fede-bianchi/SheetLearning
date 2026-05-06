using Microsoft.AspNetCore.Http;
using MusicApp.Application.Interfaces;

namespace MusicApp.Web.Infrastructure;

public class RefreshCookieHelper : IRefreshCookieHelper
{
    private const string CookieName = "refresh_token";

    public void SetRefreshCookie(HttpResponse response, string token, DateTime expiresAt)
    {
        response.Cookies.Append(CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = new DateTimeOffset(expiresAt),
            Path = "/api/auth"
        });
    }

    public void ClearRefreshCookie(HttpResponse response)
    {
        response.Cookies.Delete(CookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth"
        });
    }

    public string? ReadRefreshToken(HttpRequest request)
    {
        return request.Cookies.TryGetValue(CookieName, out var token) ? token : null;
    }
}

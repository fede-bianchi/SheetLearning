using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MusicApp.Web.Pages;

public class LoginModel : PageModel
{
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // POST to /api/auth/login will be handled by the API controller
        // This Razor Page serves the HTML form; actual auth is done via JS fetch to the API
        return Page();
    }
}

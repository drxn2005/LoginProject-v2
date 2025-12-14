using Microsoft.AspNetCore.Mvc;

namespace LoginProject.Controllers
{
    public class ThemeController : Controller
    {
        [HttpPost]
        public IActionResult SetTheme(string theme, string returnUrl)
        {
            if (!string.IsNullOrEmpty(theme) &&
                (theme == "light" || theme == "dark" || theme == "system"))
            {
                Response.Cookies.Append("theme", theme, new CookieOptions
                {
                    HttpOnly = false,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });
            }

            return LocalRedirect(returnUrl);
        }
    }
}
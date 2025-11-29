using Microsoft.AspNetCore.Identity;

namespace NetworkCafesControllers.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public int? userid { get; set; }

        /// <summary>
        /// "light" / "dark" / "system"
        /// </summary>
        public string? ThemePreference { get; set; }

    }
}

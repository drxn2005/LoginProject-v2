using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(50)]
        public string? ThemePreference { get; set; } = "system";

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public DateTimeOffset? LastLoginDate { get; set; }

        [StringLength(200)]
        public string? FullName { get; set; }

        [StringLength(500)]
        public string? ProfileImageUrl { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTimeOffset? DeletedAt { get; set; }
        public int? userid { get; set; }


    }
}

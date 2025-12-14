namespace LoginProject.Models.Entities
{
    public class AppUser
    {
        public int Id { get; set; }

        public string UserName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; 

        public string Role { get; set; } = "user"; // Admin / Owner / user

        public bool IsActive { get; set; } = true;

        public int? CafeId { get; set; }
    }
}

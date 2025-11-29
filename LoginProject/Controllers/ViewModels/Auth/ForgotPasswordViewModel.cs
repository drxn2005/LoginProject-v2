using System.ComponentModel.DataAnnotations;

namespace NetworkCafesControllers.Models.ViewModels.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;
    }
}

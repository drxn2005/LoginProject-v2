using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "اسم المستخدم أو البريد")]
        public string UserNameOrEmail { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = null!;

        [Display(Name = "تذكرني")]
        public bool RememberMe { get; set; }
    }
}

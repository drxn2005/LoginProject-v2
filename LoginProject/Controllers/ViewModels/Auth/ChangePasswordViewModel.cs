using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.ViewModels.Auth
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الحالية")]
        public string OldPassword { get; set; } = null!;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور الجديدة")]
        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة وتأكيدها غير متطابقين.")]
        public string ConfirmPassword { get; set; } = null!;
    }
}

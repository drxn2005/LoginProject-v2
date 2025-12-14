using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.ViewModels.Admin
{
    public class AdminResetPasswordViewModel
    {
        [Required(ErrorMessage = "معرف المستخدم مطلوب")]
        public string UserId { get; set; } = string.Empty;

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
        [StringLength(100, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل {2} أحرف.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور الجديدة")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("NewPassword", ErrorMessage = "كلمتا المرور غير متطابقتين")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
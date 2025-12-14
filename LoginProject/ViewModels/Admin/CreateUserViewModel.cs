using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.ViewModels.Admin
{
    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "اسم المستخدم يجب أن يكون بين 3 و 50 حرفًا")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        [Phone(ErrorMessage = "رقم الهاتف غير صالح")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب أن تكون على الأقل 6 أحرف")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "تأكيد البريد الإلكتروني؟")]
        public bool EmailConfirmed { get; set; } = true;

        [Display(Name = "الأدوار")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<string> AvailableRoles { get; set; } = new() { "Admin", "User" };
    }
}
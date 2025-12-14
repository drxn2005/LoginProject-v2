using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [Display(Name = "رقم الهاتف")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "كلمتا المرور غير متطابقتين.")]
        [Display(Name = "تأكيد كلمة المرور")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "الكافيه (اختياري)")]
        public int? CafeId { get; set; }
    }
}

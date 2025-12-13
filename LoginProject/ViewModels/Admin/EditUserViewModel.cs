using System.ComponentModel.DataAnnotations;

namespace NetworkCafesControllers.Models.ViewModels.Admin
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

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

        [Display(Name = "تأكيد البريد الإلكتروني؟")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "محظور؟")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "تاريخ انتهاء القفل")]
        [DataType(DataType.DateTime)]
        public DateTime? LockoutEnd { get; set; }

        [Display(Name = "الأدوار")]
        public List<string> SelectedRoles { get; set; } = new();

        public List<string> AvailableRoles { get; set; } = new() { "Admin", "User" };
    }
}
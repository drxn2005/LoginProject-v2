using System.ComponentModel.DataAnnotations;

namespace NetworkCafesControllers.Models.ViewModels.Admin
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "اسم المستخدم")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "رقم الهاتف")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "مفعل؟")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "محظور؟")]
        public bool IsLockedOut { get; set; }

        [Display(Name = "تاريخ القفل")]
        public DateTimeOffset? LockoutEnd { get; set; }

        [Display(Name = "عدد محاولات الفشل")]
        public int AccessFailedCount { get; set; }

        [Display(Name = "الأدوار")]
        public List<string> Roles { get; set; } = new();

        [Display(Name = "تاريخ التسجيل")]
        public DateTimeOffset RegistrationDate { get; set; }

        [Display(Name = "آخر تسجيل دخول")]
        public DateTimeOffset? LastLoginDate { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace NetworkCafesControllers.Models.ViewModels
{
    public class BrandViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العلامة التجارية مطلوب")]
        [Display(Name = "الاسم")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون بين 2 و 100 حرف", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "الوصف")]
        [StringLength(500, ErrorMessage = "الوصف يجب أن يكون أقل من 500 حرف")]
        public string? Description { get; set; }

        [Display(Name = "شعار العلامة")]
        [Url(ErrorMessage = "الرجاء إدخال رابط صحيح")]
        public string? LogoUrl { get; set; }

        [Display(Name = "الموقع الإلكتروني")]
        [Url(ErrorMessage = "الرجاء إدخال رابط موقع صحيح")]
        public string? Website { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; }

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "تاريخ التحديث")]
        public DateTime UpdatedAt { get; set; }
    }
}
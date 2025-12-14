using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;  // أضف هذا السطر
using Microsoft.AspNetCore.Http;

namespace LoginProject.Models
{
    public class Brand
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم العلامة التجارية مطلوب")]
        [StringLength(100, ErrorMessage = "الاسم يجب أن يكون بين 2 و 100 حرف", MinimumLength = 2)]
        [Display(Name = "اسم العلامة التجارية")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "الوصف يجب أن يكون أقل من 500 حرف")]
        [Display(Name = "الوصف")]
        public string? Description { get; set; }

        [Url(ErrorMessage = "الرجاء إدخال رابط صحيح")]
        [Display(Name = "رابط الشعار")]
        public string? LogoUrl { get; set; }

        [NotMapped]  // أضف هذا السطر - مهم جداً!
        [Display(Name = "شعار العلامة")]
        public IFormFile? LogoFile { get; set; }  // للرفع فقط

        [Url(ErrorMessage = "الرجاء إدخال رابط موقع صحيح")]
        [Display(Name = "الموقع الإلكتروني")]
        public string? Website { get; set; }

        [Display(Name = "نشط")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "تاريخ التحديث")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "تم الحذف")]
        public bool IsDeleted { get; set; } = false;

        [Display(Name = "تاريخ الحذف")]
        public DateTime? DeletedAt { get; set; }

        // Foreign key for who created/updated
        public string? CreatedByUserId { get; set; }
        public string? UpdatedByUserId { get; set; }
    }
}
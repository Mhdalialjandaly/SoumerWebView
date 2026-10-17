using System.ComponentModel.DataAnnotations;

namespace SoumerMVCView.Models
{
    public class CourseFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الكورس مطلوب")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "اسم الكورس بين 2 و 200 حرف")]
        public string Name { get; set; }

        [StringLength(2000, ErrorMessage = "الوصف لا يتجاوز 2000 حرف")]
        public string Description { get; set; }

        [Required(ErrorMessage = "السعر الأصلي مطلوب")]
        [Range(0, 100000000, ErrorMessage = "السعر يجب أن يكون بين 0 و 100,000,000")]
        public decimal Price { get; set; }

        /// <summary>سعر العرض (اختياري)</summary>
        [Range(0, 100000000, ErrorMessage = "سعر العرض يجب أن يكون بين 0 و 100,000,000")]
        public decimal? DiscountPrice { get; set; }

        /// <summary>تاريخ ووقت انتهاء العرض (اختياري)</summary>
        public DateTime? DiscountEndDate { get; set; }

        /// <summary>للقراءة فقط - لا يتم تعديله من النموذج</summary>
        public DateTime CreatedAt { get; set; }

        public string CreatedAtDisplay { get; set; }
    }


}
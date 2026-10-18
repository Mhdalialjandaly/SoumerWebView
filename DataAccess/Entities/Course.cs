using Core;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities
{
    public class Course : IEntity
    {
        public Course()
        {
            TeacherCourses = new HashSet<TeacherCourse>();
            CourseRegistrations = new HashSet<CourseRegistration>();
            CourseVideos = new HashSet<CourseVideo>();
            CoursePdfs = new HashSet<CoursePdf>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>السعر الأصلي للكورس</summary>
        public decimal Price { get; set; }

        /// <summary>سعر العرض الترويجي (إن وجد)</summary>
        public decimal? DiscountPrice { get; set; }

        /// <summary>تاريخ ووقت انتهاء العرض</summary>
        public DateTime? DiscountEndDate { get; set; }

        /// <summary>هل العرض ساري حالياً؟</summary>
        [NotMapped]
        public bool HasActiveDiscount =>
            DiscountPrice.HasValue
            && DiscountPrice.Value > 0
            && DiscountPrice.Value < Price
            && DiscountEndDate.HasValue
            && DiscountEndDate.Value > DateTime.Now;

        /// <summary>السعر الفعلي الذي يدفعه المستخدم (مع مراعاة العرض)</summary>
        [NotMapped]
        public decimal EffectivePrice => HasActiveDiscount ? DiscountPrice.Value : Price;

        /// <summary>المبلغ الموفّر (إن وجد عرض)</summary>
        [NotMapped]
        public decimal DiscountAmount => HasActiveDiscount ? Price - DiscountPrice.Value : 0;

        /// <summary>نسبة الخصم (0-100)</summary>
        [NotMapped]
        public int DiscountPercent =>
            HasActiveDiscount && Price > 0
                ? (int)Math.Round((Price - DiscountPrice.Value) / Price * 100)
                : 0;

        public virtual ICollection<TeacherCourse> TeacherCourses { get; set; }
        public virtual ICollection<CourseRegistration> CourseRegistrations { get; set; }
        public virtual ICollection<CourseVideo> CourseVideos { get; set; }
        public virtual ICollection<CoursePdf> CoursePdfs { get; set; } 
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}
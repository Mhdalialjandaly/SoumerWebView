using Core;

namespace DataAccess.Entities
{
    public class CoursePdf : IEntity
    {
        public int Id { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        /// <summary>المسار النسبي داخل wwwroot مثلاً /uploads/pdfs/course-1/abc.pdf</summary>
        public string FilePath { get; set; }

        /// <summary>اسم الملف الأصلي عند الرفع</summary>
        public string OriginalFileName { get; set; }

        /// <summary>حجم الملف بالبايت</summary>
        public long FileSize { get; set; }

        /// <summary>ترتيب الظهور</summary>
        public int Order { get; set; }

        /// <summary>هل الملف متاح للطلاب؟</summary>
        public bool IsPublished { get; set; } = true;

        /// <summary>مجاني (يمكن تحميله حتى بدون شراء) — افتراضياً لا</summary>
        public bool IsFree { get; set; } = false;

        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; }
        public string DeletedBy { get; set; }
    }
}
using Models;

namespace SoumerMVCView.Services.CourseService
{
    public interface ICourseService
    {
        Task<List<CourseDto>> GetAllCourses();
        Task<CourseDto> GetCourseById(int courseId);
        Task<List<CourseDto>> GetAvailableCourses();
        Task<bool> EnrollInCourse(int courseId, string userId);
        Task<bool> UnenrollFromCourse(int courseId, string userId);
        Task<List<CourseRegistrationDto>> GetUserEnrolledCourses(string userId);
        Task<bool> IsUserEnrolled(int courseId, string userId);
        Task<CourseEnrollmentResult> CheckEnrollmentEligibility(int courseId, string userId);
    }

    public class CourseEnrollmentResult
    {
        public bool IsEligible { get; set; }
        public string Message { get; set; }

        /// <summary>السعر الفعلي المطلوب دفعه (بعد تطبيق العرض إن وُجد)</summary>
        public decimal CoursePrice { get; set; }

        /// <summary>السعر الأصلي قبل العرض</summary>
        public decimal OriginalPrice { get; set; }

        /// <summary>هل يوجد عرض ساري حالياً؟</summary>
        public bool HasActiveDiscount { get; set; }

        /// <summary>مبلغ الخصم (إن وُجد عرض)</summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>نسبة الخصم (0-100)</summary>
        public int DiscountPercent { get; set; }

        public decimal UserBalance { get; set; }
        public bool IsAlreadyEnrolled { get; set; }
    }
}
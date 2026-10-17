using Models;

namespace SoumerMVCView.Models
{
    public class CourseListViewModel
    {
        public List<CourseDto> Courses { get; set; }
        public decimal UserBalance { get; set; }
        public int TotalCourses { get; set; }
        public int EnrolledCoursesCount { get; set; }
    }

    public class CourseDetailViewModel
    {
        public CourseDto Course { get; set; }

        // ====== حالة المستخدم ======
        public bool IsEnrolled { get; set; }
        public bool CanEnroll { get; set; }
        public string EnrollmentMessage { get; set; }
        public decimal UserBalance { get; set; }

        // ====== تفاصيل إضافية ======
        public List<TeacherDto> Teachers { get; set; } = new();
        public int TotalVideos { get; set; }
        public int FreeVideos { get; set; }

        // ====== بيانات العرض ======
        public decimal EffectivePrice => Course?.EffectivePrice ?? 0;
        public bool HasActiveDiscount => Course?.HasActiveDiscount ?? false;

        // حساب الأيام المتبقية على العرض
        public int DiscountDaysLeft
        {
            get
            {
                if (!HasActiveDiscount || !Course?.DiscountEndDate.HasValue == true)
                    return 0;
                var diff = Course.DiscountEndDate.Value - DateTime.Now;
                return diff.TotalDays > 0 ? (int)Math.Ceiling(diff.TotalDays) : 0;
            }
        }
    }

    public class MyCoursesViewModel
    {
        public List<CourseRegistrationDto> EnrolledCourses { get; set; }
        public int TotalEnrolled { get; set; }
        public decimal TotalSpent { get; set; }

        // إضافة هذه الخصائص الجديدة
        public Dictionary<int, List<CourseVideoDto>> CourseVideos { get; set; }
        public Dictionary<int, int> CourseVideosCount { get; set; }
        public int TotalVideos { get; set; }
    }
}

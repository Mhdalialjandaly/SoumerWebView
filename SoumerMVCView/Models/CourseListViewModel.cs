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
        public bool IsEnrolled { get; set; }
        public decimal UserBalance { get; set; }
        public bool CanEnroll { get; set; }
        public string EnrollmentMessage { get; set; }
        public List<TeacherDto> Teachers { get; set; }
    }

    public class MyCoursesViewModel
    {
        public List<CourseRegistrationDto> EnrolledCourses { get; set; }
        public int TotalEnrolled { get; set; }
        public decimal TotalSpent { get; set; }
    }
}

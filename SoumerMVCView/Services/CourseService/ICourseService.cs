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
        public decimal CoursePrice { get; set; }
        public decimal UserBalance { get; set; }
        public bool IsAlreadyEnrolled { get; set; }
    }
}
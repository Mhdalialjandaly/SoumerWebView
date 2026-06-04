using DataAccess.Entities;

namespace SoumerMVCView.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalTeachers { get; set; }
        public int TotalCourses { get; set; }
        public int TotalVideos { get; set; }
        public int TotalTeacherCourses { get; set; }
        public List<Teacher> RecentTeachers { get; set; }
        public List<Course> RecentCourses { get; set; }
    }
}

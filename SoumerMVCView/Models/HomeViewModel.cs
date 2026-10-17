using Models;

namespace SoumerMVCView.Models
{
    public class HomeViewModel
    {
        public List<TeacherDto> FeaturedTeachers { get; set; }
        public List<GradeTeachersDto> GradesTeachers { get; set; }
        public HashSet<int> EnrolledCourseIds { get; set; } = new();
    }
}

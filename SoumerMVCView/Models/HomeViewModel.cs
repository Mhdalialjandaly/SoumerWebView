using Models;

namespace SoumerMVCView.Models
{
    public class HomeViewModel
    {
        public List<TeacherDto> FeaturedTeachers { get; set; }
        public List<GradeTeachersDto> GradesTeachers { get; set; }
    }
}

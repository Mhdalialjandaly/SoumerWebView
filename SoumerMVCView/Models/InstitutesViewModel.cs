using Models;

namespace SoumerMVCView.Models
{
    public class InstitutesViewModel
    {
        public List<TeacherDto> Teachers { get; set; }
        public List<GradeTeachersDto> GradesTeachers { get; set; }
        public int TotalInstitutes => Teachers?.Count ?? 0;
        public int TotalActiveTeachers => Teachers?.Count(t => t.IsActive) ?? 0;
    }
}
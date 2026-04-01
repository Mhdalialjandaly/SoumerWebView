using Models;

namespace SoumerMVCView.Models
{
    public class ManageTeacherAssignmentsViewModel
    {
        public List<TeacherDto> Teachers { get; set; }
        public List<GradeAssignmentsDto> GradesAssignments { get; set; }
        public AssignTeacherFormModel AssignForm { get; set; }
    }

    public class GradeAssignmentsDto
    {
        public int Grade { get; set; }
        public string GradeLevel { get; set; }
        public List<TeacherAssignmentDto> AssignedTeachers { get; set; }
        public int RequiredTeachersCount { get; set; }
    }

    public class TeacherAssignmentDto
    {
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public string Subject { get; set; }
        public int Priority { get; set; }
        public string Image { get; set; }
    }

    public class AssignTeacherFormModel
    {
        public int TeacherId { get; set; }
        public int Grade { get; set; }
        public int Priority { get; set; }
        public List<TeacherDto> AvailableTeachers { get; set; }
        public List<int> AvailableGrades { get; set; }
    }
}

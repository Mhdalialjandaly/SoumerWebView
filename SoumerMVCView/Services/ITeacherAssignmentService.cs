using Models;

namespace SoumerMVCView.Services
{
    public interface ITeacherAssignmentService
    {
        Task<List<GradeTeachersDto>> GetOrganizedGradesTeachers();
        Task<bool> AssignTeacherToGrade(int teacherId, int grade, int priority);
        Task<bool> RemoveTeacherFromGrade(int teacherId, int grade);
        Task<bool> UpdateTeacherPriority(int teacherId, int grade, int newPriority);
    }
}

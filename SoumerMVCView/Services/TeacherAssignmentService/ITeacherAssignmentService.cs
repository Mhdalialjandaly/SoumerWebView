using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Services.TeacherAssignmentService
{
    public interface ITeacherAssignmentService
    {
        Task<List<GradeTeachersDto>> GetOrganizedGradesTeachers();
        Task<bool> AssignTeacherToGrade(int teacherId, int grade, int priority);
        Task<bool> RemoveTeacherFromGrade(int teacherId, int grade);
        Task<bool> UpdateTeacherPriority(int teacherId, int grade, int newPriority);
        Task<List<GradeAssignmentsDto>> GetAllGradesAssignments();
        Task<List<TeacherDto>> GetAvailableTeachersForGrade(int grade);
        Task<int> GetNextPriorityForGrade(int grade);
    }
}

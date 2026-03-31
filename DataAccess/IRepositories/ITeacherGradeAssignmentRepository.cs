using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface ITeacherGradeAssignmentRepository : IBaseRepository<TeacherGradeAssignmentDto, TeacherGradeAssignment>, IInjectable
    {
        Task<List<TeacherGradeAssignmentDto>> GetAssignmentsByGrade(int grade);
        Task<TeacherGradeAssignmentDto> GetTeacherAssignment(int teacherId, int grade);
    }
}
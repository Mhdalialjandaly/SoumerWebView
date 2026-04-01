using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface ICourseRepository : IBaseRepository<CourseDto, Course>, IInjectable
    {
        Task<List<CourseDto>> GetAvailableCourses();
        Task<CourseDto> GetCourseWithDetails(int courseId);
        Task<List<CourseDto>> GetCoursesByTeacher(int teacherId);
        Task<bool> IsUserEnrolled(int courseId, string userId);
    }
}

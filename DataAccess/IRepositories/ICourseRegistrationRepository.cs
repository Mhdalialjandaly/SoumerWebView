using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface ICourseRegistrationRepository : IBaseRepository<CourseRegistrationDto, CourseRegistration>, IInjectable
    {
        Task<List<CourseRegistrationDto>> GetUserRegistrations(string userId);
        Task<CourseRegistrationDto> GetRegistration(int courseId, string userId);
        Task<bool> IsUserEnrolled(int courseId, string userId);
        Task<int> GetUserEnrolledCoursesCount(string userId);
    }
}

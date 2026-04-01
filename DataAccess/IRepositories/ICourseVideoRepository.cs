using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface ICourseVideoRepository : IBaseRepository<CourseVideoDto, CourseVideo>, IInjectable
    {
        Task<List<CourseVideoDto>> GetVideosByCourseId(int courseId);
        Task<CourseVideoDto> GetVideoWithCourse(int videoId);
    }
}

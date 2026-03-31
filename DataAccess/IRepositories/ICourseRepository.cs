using Core.Interface;
using DataAccess.Base;
using DataAccess.Entities;
using Models;

namespace DataAccess.IRepositories
{
    public interface ICourseRepository : IBaseRepository<CourseDto, Course>, IInjectable
    {
    }
}

using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Models;

namespace DataAccess.Repositories
{
    public class CourseRepository : BaseRepository<CourseDto, Course>, ICourseRepository
    {
        public CourseRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}

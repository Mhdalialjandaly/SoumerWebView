using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Models;

namespace DataAccess.Repositories
{
    public class CourseRegistrationRepository : BaseRepository<CourseRegistrationDto, CourseRegistration>, ICourseRegistrationRepository
    {
        public CourseRegistrationRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}

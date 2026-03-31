using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Models;

namespace DataAccess.Repositories
{
    public class TeacherCourseRepository : BaseRepository<TeacherCourseDto, TeacherCourse>, ITeacherCourseRepository
    {
        public TeacherCourseRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}

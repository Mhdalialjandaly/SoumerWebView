using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Models;

namespace DataAccess.Repositories
{
    public class TeacherRepository : BaseRepository<TeacherDto, Teacher>, ITeacherRepository
    {
        public TeacherRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}

using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Models;

namespace DataAccess.Repositories
{
    public class CoursePdfRepository : BaseRepository<CoursePdfDto, CoursePdf>, ICoursePdfRepository
    {
        public CoursePdfRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
        }
    }
}

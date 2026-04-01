using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class CourseVideoRepository : BaseRepository<CourseVideoDto, CourseVideo>, ICourseVideoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public CourseVideoRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CourseVideoDto>> GetVideosByCourseId(int courseId)
        {
            var videos = await _context.Set<CourseVideo>()
                .Where(v => v.CourseId == courseId && v.DeletedAt == null)
                .OrderBy(v => v.Order)
                .ToListAsync();

            return _mapper.Map<List<CourseVideoDto>>(videos);
        }

        public async Task<CourseVideoDto> GetVideoWithCourse(int videoId)
        {
            var video = await _context.Set<CourseVideo>()
                .Include(v => v.Course)
                .FirstOrDefaultAsync(v => v.Id == videoId && v.DeletedAt == null);

            return _mapper.Map<CourseVideoDto>(video);
        }
    }
}

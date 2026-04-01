using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class TeacherCourseRepository : BaseRepository<TeacherCourseDto, TeacherCourse>, ITeacherCourseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        public TeacherCourseRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
            _context = context;
            _mapper = mapper;
        }
        public async Task<List<TeacherCourseDto>> GetTeacherCoursesWithDetails(int teacherId)
        {
            var teacherCourses = await _context.Set<TeacherCourse>()
                .Include(tc => tc.Course)
                .Where(tc => tc.TeacherId == teacherId && tc.DeletedAt == null)
                .ToListAsync();

            return _mapper.Map<List<TeacherCourseDto>>(teacherCourses);
        }
    }
}

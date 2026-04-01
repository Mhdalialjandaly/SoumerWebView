using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class CourseRepository : BaseRepository<CourseDto, Course>, ICourseRepository
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        public CourseRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
            _mapper = mapper;
            _context = context;
        }
        public async Task<List<CourseDto>> GetAvailableCourses()
        {
            try
            {
                var courses = await _context.Set<Course>()
                    .Where(c => c.DeletedAt == null)
                    .Include(c => c.TeacherCourses)
                        .ThenInclude(tc => tc.Teacher)
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                return _mapper.Map<List<CourseDto>>(courses);
            }
            catch (Exception)
            {
                return new List<CourseDto>();
            }
        }

        public async Task<CourseDto> GetCourseWithDetails(int courseId)
        {
            try
            {
                var course = await _context.Set<Course>()
                    .Where(c => c.Id == courseId && c.DeletedAt == null)
                    .Include(c => c.TeacherCourses)
                        .ThenInclude(tc => tc.Teacher)
                    .FirstOrDefaultAsync();

                return _mapper.Map<CourseDto>(course);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<CourseDto>> GetCoursesByTeacher(int teacherId)
        {
            try
            {
                var courses = await _context.Set<Course>()
                    .Where(c => c.DeletedAt == null)
                    .Include(c => c.TeacherCourses)
                        .Where(c => c.TeacherCourses.Any(tc => tc.TeacherId == teacherId && tc.DeletedAt == null))
                    .ToListAsync();

                return _mapper.Map<List<CourseDto>>(courses);
            }
            catch (Exception)
            {
                return new List<CourseDto>();
            }
        }

        public async Task<bool> IsUserEnrolled(int courseId, string userId)
        {
            try
            {
                var registration = await _context.Set<CourseRegistration>()
                    .FirstOrDefaultAsync(r => r.CourseId == courseId && r.UserId == userId && r.DeletedAt == null);

                return registration != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
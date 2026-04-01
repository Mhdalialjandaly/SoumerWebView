using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class CourseRegistrationRepository : BaseRepository<CourseRegistrationDto, CourseRegistration>, ICourseRegistrationRepository
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        public CourseRegistrationRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
            _mapper = mapper;
            _context = context;
        }
        public async Task<List<CourseRegistrationDto>> GetUserRegistrations(string userId)
        {
            try
            {
                var registrations = await _context.Set<CourseRegistration>()
                    .Where(r => r.UserId == userId && r.DeletedAt == null)
                    .Include(r => r.Course)
                        .ThenInclude(c => c.TeacherCourses)
                            .ThenInclude(tc => tc.Teacher)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync();

                return _mapper.Map<List<CourseRegistrationDto>>(registrations);
            }
            catch (Exception)
            {
                return new List<CourseRegistrationDto>();
            }
        }

        public async Task<CourseRegistrationDto> GetRegistration(int courseId, string userId)
        {
            try
            {
                var registration = await _context.Set<CourseRegistration>()
                    .FirstOrDefaultAsync(r => r.CourseId == courseId && r.UserId == userId && r.DeletedAt == null);

                return _mapper.Map<CourseRegistrationDto>(registration);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> IsUserEnrolled(int courseId, string userId)
        {
            try
            {
                var registration = await _context.Set<CourseRegistration>()
                    .AnyAsync(r => r.CourseId == courseId && r.UserId == userId && r.DeletedAt == null);

                return registration;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<int> GetUserEnrolledCoursesCount(string userId)
        {
            try
            {
                var count = await _context.Set<CourseRegistration>()
                    .CountAsync(r => r.UserId == userId && r.DeletedAt == null);

                return count;
            }
            catch (Exception)
            {
                return 0;
            }
        }
    }
}
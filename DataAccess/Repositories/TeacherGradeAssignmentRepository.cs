using AutoMapper;
using DataAccess.Base;
using DataAccess.Entities;
using DataAccess.IRepositories;
using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories
{
    public class TeacherGradeAssignmentRepository : BaseRepository<TeacherGradeAssignmentDto, TeacherGradeAssignment>, ITeacherGradeAssignmentRepository
    {
        private readonly IMapper _mapper;
        private readonly ApplicationDbContext _context;
        public TeacherGradeAssignmentRepository(IMapper mapper, ApplicationDbContext context) : base(mapper, context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task<List<TeacherGradeAssignmentDto>> GetAssignmentsByGrade(int grade)
        {
            try
            {
                var assignments = await _context.Set<TeacherGradeAssignment>()
                    .Include(a => a.Teacher)
                    .Where(a => a.Grade == grade && a.DeletedAt == null)
                    .OrderBy(a => a.Priority)
                    .ThenBy(a => a.Teacher.Name)
                    .ToListAsync();

                return _mapper.Map<List<TeacherGradeAssignmentDto>>(assignments);
            }
            catch (Exception)
            {
                return new List<TeacherGradeAssignmentDto>();
            }
        }

        public async Task<TeacherGradeAssignmentDto> GetTeacherAssignment(int teacherId, int grade)
        {
            try
            {
                var assignment = await _context.Set<TeacherGradeAssignment>()
                    .Include(a => a.Teacher)
                    .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Grade == grade && a.DeletedAt == null);

                return _mapper.Map<TeacherGradeAssignmentDto>(assignment);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
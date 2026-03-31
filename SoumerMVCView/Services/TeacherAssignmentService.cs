using DataAccess.IRepositories;
using Models;
using SoumerMVCView.Services;

namespace Services
{
    public class TeacherAssignmentService : ITeacherAssignmentService
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherGradeAssignmentRepository _assignmentRepository;

        public TeacherAssignmentService(
            ITeacherRepository teacherRepository,
            ITeacherGradeAssignmentRepository assignmentRepository)
        {
            _teacherRepository = teacherRepository;
            _assignmentRepository = assignmentRepository;
        }

        public async Task<List<GradeTeachersDto>> GetOrganizedGradesTeachers()
        {
            var gradesTeachers = new List<GradeTeachersDto>();

            for (int grade = 1; grade <= 12; grade++)
            {
                // جلب التوزيعات المنظمة من قاعدة البيانات
                var assignments = await _assignmentRepository.GetAssignmentsByGrade(grade);

                // تحديد عدد الأساتذة المطلوبين لكل صف
                int requiredTeachersCount = grade == 12 ? 5 : (grade % 3 == 0 ? 4 : 3);

                // إذا كان عدد الأساتذة في قاعدة البيانات أقل من المطلوب
                if (assignments.Count < requiredTeachersCount)
                {
                    // يمكن إضافة منطق لتعبئة الأساتذة المتبقين من الأساتذة المتاحين
                    assignments = await FillRemainingTeachers(assignments, grade, requiredTeachersCount);
                }

                string gradeLevel = grade <= 6 ? "الابتدائية" : (grade <= 9 ? "المتوسطة" : "الثانوية");

                gradesTeachers.Add(new GradeTeachersDto
                {
                    Grade = grade,
                    GradeLevel = gradeLevel,
                    Teachers = assignments.Take(requiredTeachersCount)
                        .Select(a => a.Teacher)
                        .ToList()
                });
            }

            return gradesTeachers;
        }

        private async Task<List<TeacherGradeAssignmentDto>> FillRemainingTeachers(
            List<TeacherGradeAssignmentDto> currentAssignments,
            int grade,
            int requiredCount)
        {
            var allTeachers = await _teacherRepository.GetAll();
            var assignedTeacherIds = currentAssignments.Select(a => a.TeacherId).ToHashSet();

            var availableTeachers = allTeachers
                .Where(t => !assignedTeacherIds.Contains(t.Id))
                .Take(requiredCount - currentAssignments.Count)
                .ToList();

            // إنشاء توزيعات مؤقتة للأساتذة المتبقين
            var tempAssignments = new List<TeacherGradeAssignmentDto>();
            int nextPriority = currentAssignments.Count > 0 ?
                currentAssignments.Max(a => a.Priority) + 1 : 1;

            foreach (var teacher in availableTeachers)
            {
                tempAssignments.Add(new TeacherGradeAssignmentDto
                {
                    TeacherId = teacher.Id,
                    Teacher = teacher,
                    Grade = grade,
                    Priority = nextPriority++
                });
            }

            currentAssignments.AddRange(tempAssignments);
            return currentAssignments;
        }

        public async Task<bool> AssignTeacherToGrade(int teacherId, int grade, int priority)
        {
            try
            {
                var existingAssignment = await _assignmentRepository.GetTeacherAssignment(teacherId, grade);

                if (existingAssignment != null)
                {
                    // تحديث الأولوية إذا كان موجوداً
                    existingAssignment.Priority = priority;
                    await _assignmentRepository.Update(existingAssignment);
                }
                else
                {
                    // إنشاء توزيع جديد
                    var newAssignment = new TeacherGradeAssignmentDto
                    {
                        TeacherId = teacherId,
                        Grade = grade,
                        Priority = priority
                    };
                    await _assignmentRepository.Add(newAssignment);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> RemoveTeacherFromGrade(int teacherId, int grade)
        {
            try
            {
                var assignment = await _assignmentRepository.GetTeacherAssignment(teacherId, grade);

                if (assignment != null)
                {
                    await _assignmentRepository.Delete(assignment.Id);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateTeacherPriority(int teacherId, int grade, int newPriority)
        {
            try
            {
                var assignment = await _assignmentRepository.GetTeacherAssignment(teacherId, grade);

                if (assignment != null)
                {
                    assignment.Priority = newPriority;
                    await _assignmentRepository.Update(assignment);
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
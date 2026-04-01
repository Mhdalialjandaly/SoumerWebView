using DataAccess.IRepositories;
using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Services.TeacherAssignmentService
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
                // جلب التوزيعات المحفوظة من قاعدة البيانات فقط
                var assignments = await _assignmentRepository.GetAssignmentsByGrade(grade);

                string gradeLevel = grade <= 6 ? "الابتدائية" : grade <= 9 ? "المتوسطة" : "الثانوية";

                gradesTeachers.Add(new GradeTeachersDto
                {
                    Grade = grade,
                    GradeLevel = gradeLevel,
                    // عرض الأساتذة المحفوظين فقط حسب الأولوية
                    Teachers = assignments
                        .OrderBy(a => a.Priority)
                        .Select(a => a.Teacher)
                        .ToList()
                });
            }

            return gradesTeachers;
        }

        //private async Task<List<TeacherGradeAssignmentDto>> FillRemainingTeachers(
        //    List<TeacherGradeAssignmentDto> currentAssignments,
        //    int grade,
        //    int requiredCount)
        //{
        //    var allTeachers = await _teacherRepository.GetAll();
        //    var assignedTeacherIds = currentAssignments.Select(a => a.TeacherId).ToHashSet();

        //    var availableTeachers = allTeachers
        //        .Where(t => !assignedTeacherIds.Contains(t.Id))
        //        .Take(requiredCount - currentAssignments.Count)
        //        .ToList();

        //    // إنشاء توزيعات مؤقتة للأساتذة المتبقين
        //    var tempAssignments = new List<TeacherGradeAssignmentDto>();
        //    int nextPriority = currentAssignments.Count > 0 ?
        //        currentAssignments.Max(a => a.Priority) + 1 : 1;

        //    foreach (var teacher in availableTeachers)
        //    {
        //        tempAssignments.Add(new TeacherGradeAssignmentDto
        //        {
        //            TeacherId = teacher.Id,
        //            Teacher = teacher,
        //            Grade = grade,
        //            Priority = nextPriority++
        //        });
        //    }

        //    currentAssignments.AddRange(tempAssignments);
        //    return currentAssignments;
        //}

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
        public async Task<List<GradeAssignmentsDto>> GetAllGradesAssignments()
        {
            var gradesAssignments = new List<GradeAssignmentsDto>();

            for (int grade = 1; grade <= 12; grade++)
            {
                var assignments = await _assignmentRepository.GetAssignmentsByGrade(grade);
                int requiredCount = grade == 12 ? 5 : grade % 3 == 0 ? 4 : 3;
                string gradeLevel = grade <= 6 ? "الابتدائية" : grade <= 9 ? "المتوسطة" : "الثانوية";

                gradesAssignments.Add(new GradeAssignmentsDto
                {
                    Grade = grade,
                    GradeLevel = gradeLevel,
                    RequiredTeachersCount = requiredCount,
                    AssignedTeachers = assignments.Select(a => new TeacherAssignmentDto
                    {
                        TeacherId = a.Teacher.Id,
                        TeacherName = a.Teacher.Name,
                        Subject = a.Teacher.Subject,
                        Priority = a.Priority,
                        Image = a.Teacher.Image
                    }).OrderBy(t => t.Priority).ToList()
                });
            }

            return gradesAssignments;
        }

        public async Task<List<TeacherDto>> GetAvailableTeachersForGrade(int grade)
        {
            var allTeachers = await _teacherRepository.GetAll();
            var assignedTeachers = await _assignmentRepository.GetAssignmentsByGrade(grade);
            var assignedTeacherIds = assignedTeachers.Select(a => a.TeacherId).ToHashSet();

            return allTeachers.Where(t => !assignedTeacherIds.Contains(t.Id)).ToList();
        }

        public async Task<int> GetNextPriorityForGrade(int grade)
        {
            var assignments = await _assignmentRepository.GetAssignmentsByGrade(grade);
            return assignments.Any() ? assignments.Max(a => a.Priority) + 1 : 1;
        }
    }
}
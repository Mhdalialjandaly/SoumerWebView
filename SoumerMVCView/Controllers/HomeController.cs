using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Models;
using SoumerMVCView.Services.TeacherAssignmentService;
using System.Diagnostics;

namespace SoumerMVCView.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherAssignmentService _assignmentService;
        private readonly ITeacherCourseRepository _teacherCourseRepository;

        public HomeController(
            ILogger<HomeController> logger,
            ITeacherRepository teacherRepository,
            ITeacherAssignmentService assignmentService,
            ITeacherCourseRepository teacherCourseRepository)
        {
            _logger = logger;
            _teacherRepository = teacherRepository;
            _assignmentService = assignmentService;
            _teacherCourseRepository = teacherCourseRepository;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                FeaturedTeachers = await GetFeaturedTeachers(),
                GradesTeachers = await _assignmentService.GetOrganizedGradesTeachers()
            };
            return View(model);
        }

        // إدارة توزيع الأساتذة (للأدمن)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageTeacherAssignments()
        {
            var allTeachers = await _teacherRepository.GetAll();
            var gradesAssignments = await _assignmentService.GetAllGradesAssignments();

            var availableGrades = Enumerable.Range(1, 12).ToList();

            var model = new ManageTeacherAssignmentsViewModel
            {
                Teachers = allTeachers,
                GradesAssignments = gradesAssignments,
                AssignForm = new AssignTeacherFormModel
                {
                    AvailableTeachers = allTeachers,
                    AvailableGrades = availableGrades
                }
            };

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTeacherPriority(int teacherId, int grade, int newPriority)
        {
            try
            {
                var result = await _assignmentService.UpdateTeacherPriority(teacherId, grade, newPriority);
                return Json(new { success = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAvailableTeachersForGrade(int grade)
        {
            try
            {
                var teachers = await _assignmentService.GetAvailableTeachersForGrade(grade);
                return Json(new { success = true, teachers = teachers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetNextPriority(int grade)
        {
            try
            {
                var nextPriority = await _assignmentService.GetNextPriorityForGrade(grade);
                return Json(new { success = true, priority = nextPriority });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignTeacherToGrade(int teacherId, int grade, int priority)
        {
            var result = await _assignmentService.AssignTeacherToGrade(teacherId, grade, priority);
            return Json(new { success = result });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveTeacherFromGrade(int teacherId, int grade)
        {
            var result = await _assignmentService.RemoveTeacherFromGrade(teacherId, grade);
            return Json(new { success = result });
        }

        private async Task<List<TeacherDto>> GetFeaturedTeachers()
        {
            return (await _teacherRepository.GetAll()).Take(6).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetGradesTeachersData()
        {
            var gradesTeachers = await _assignmentService.GetOrganizedGradesTeachers();
            return Json(gradesTeachers);
        }

        [HttpGet]
        public async Task<IActionResult> GetFeaturedTeachersData()
        {
            var featuredTeachers = await GetFeaturedTeachers();
            return Json(featuredTeachers);
        }

        [HttpGet]
        public async Task<IActionResult> GetTeacherCourses(int id)
        {
            try
            {
                var teacher = await _teacherRepository.GetById(id);
                if (teacher == null)
                {
                    return Json(new { success = false, message = "الأستاذ غير موجود" });
                }

                var teacherCourses = await _teacherCourseRepository.GetTeacherCoursesWithDetails(id);

                var coursesForTeacher = teacherCourses
                    .Select(tc => new
                    {
                        id = tc.CourseId,
                        name = tc.Course?.Name ?? "",
                        description = tc.Course?.Description ?? "",
                        price = tc.Course?.Price ?? 0
                    })
                    .ToList();

                var result = new
                {
                    success = true,
                    id = teacher.Id,
                    name = teacher.Name,
                    subject = teacher.Subject,
                    bio = teacher.Bio,
                    image = teacher.Image,
                    courses = coursesForTeacher,
                    teacherCourses = coursesForTeacher
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher courses for teacher ID: {TeacherId}", id);
                return Json(new { success = false, message = "حدث خطأ في تحميل بيانات المعلم" });
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
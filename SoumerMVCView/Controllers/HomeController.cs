// SoumerMVCView/Controllers/HomeController.cs
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Models;
using SoumerMVCView.Services.BalanceService;
using SoumerMVCView.Services.CourseService;
using SoumerMVCView.Services.TeacherAssignmentService;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SoumerMVCView.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherAssignmentService _assignmentService;
        private readonly ICourseService _courseService;
        private readonly IBalanceService _balanceService;

        public HomeController(
            ILogger<HomeController> logger,
            ITeacherRepository teacherRepository,
            ITeacherAssignmentService assignmentService,
            ICourseService courseService,
            IBalanceService balanceService)
        {
            _logger = logger;
            _teacherRepository = teacherRepository;
            _assignmentService = assignmentService;
            _courseService = courseService;
            _balanceService = balanceService;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {
                FeaturedTeachers = await GetFeaturedTeachers(),
                GradesTeachers = await _assignmentService.GetOrganizedGradesTeachers() // استخدام الخدمة المنظمة
            };
            return View(model);
        }

        // إضافة Action جديد لإدارة توزيع الأساتذة (للأدمن)
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

        public IActionResult Privacy()
        {
            return View();
        }
        // أضف هذه الـ Actions في HomeController.cs

        public async Task<IActionResult> MyCourses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var enrolledCourses = await _courseService.GetUserEnrolledCourses(userId);

            // حساب إجمالي النقاط المنفقة
            decimal totalSpent = 0;
            foreach (var enrollment in enrolledCourses)
            {
                totalSpent += enrollment.Course?.Price ?? 0;
            }

            var model = new MyCoursesViewModel
            {
                EnrolledCourses = enrolledCourses,
                TotalEnrolled = enrolledCourses.Count,
                TotalSpent = totalSpent
            };

            // يمكنك جلب بيانات الكورسات المسجل فيها المستخدم
            return PartialView("_MyCourses",model);
        }

        public async Task<IActionResult> Points()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var model = await _balanceService.GetUserBalance(userId);
            // يمكنك جلب بيانات النقاط
            return PartialView("_Points", model);
        }

        public IActionResult Institutes()
        {
            // يمكنك جلب بيانات المعاهد
            return PartialView("_Institutes");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
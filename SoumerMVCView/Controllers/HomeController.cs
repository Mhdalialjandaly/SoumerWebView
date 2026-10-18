using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Models;
using SoumerMVCView.Services.CourseService;
using SoumerMVCView.Services.TeacherAssignmentService;
using System.Diagnostics;
using System.Security.Claims;

namespace SoumerMVCView.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherAssignmentService _assignmentService;
        private readonly ITeacherCourseRepository _teacherCourseRepository;
        private readonly ICourseService _courseService;

        public HomeController(
            ILogger<HomeController> logger,
            ITeacherRepository teacherRepository,
            ITeacherAssignmentService assignmentService,
            ITeacherCourseRepository teacherCourseRepository,
            ICourseService courseService)
        {
            _logger = logger;
            _teacherRepository = teacherRepository;
            _assignmentService = assignmentService;
            _teacherCourseRepository = teacherCourseRepository;
            _courseService = courseService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var model = new HomeViewModel
            {
                FeaturedTeachers = await GetFeaturedTeachers(),
                GradesTeachers = await _assignmentService.GetOrganizedGradesTeachers()
            };

            // جلب المواد المشترك بها المستخدم الحالي
            if (!string.IsNullOrEmpty(userId))
            {
                try
                {
                    var enrollments = await _courseService.GetUserEnrolledCourses(userId);
                    model.EnrolledCourseIds = enrollments
                        .Select(e => e.CourseId)
                        .Distinct()
                        .ToHashSet();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load enrolled courses for user {UserId}", userId);
                }
            }

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
                    return Json(new { success = false, message = "الأستاذ غير موجود" });

                var teacherCourses = await _teacherCourseRepository.GetTeacherCoursesWithDetails(id);

                // ✅ جلب مواد المستخدم المشترك بها
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var enrolledIds = new HashSet<int>();
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var enrollments = await _courseService.GetUserEnrolledCourses(userId);
                        enrolledIds = enrollments.Select(e => e.CourseId).Distinct().ToHashSet();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load enrollments for user {UserId}", userId);
                    }
                }

                var coursesForTeacher = teacherCourses
                    .Where(tc => tc.Course != null)
                    .Select(tc =>
                    {
                        var course = tc.Course;

                        bool hasActiveDiscount =
                            course.DiscountPrice.HasValue
                            && course.DiscountPrice.Value > 0
                            && course.DiscountPrice.Value < course.Price
                            && course.DiscountEndDate.HasValue
                            && course.DiscountEndDate.Value > DateTime.Now;

                        decimal effectivePrice = hasActiveDiscount
                            ? course.DiscountPrice.Value
                            : course.Price;

                        decimal discountAmount = hasActiveDiscount
                            ? course.Price - course.DiscountPrice.Value
                            : 0;

                        int discountPercent = hasActiveDiscount && course.Price > 0
                            ? (int)Math.Round((course.Price - course.DiscountPrice.Value) / course.Price * 100)
                            : 0;

                        return new
                        {
                            id = course.Id,
                            name = course.Name ?? "",
                            description = course.Description ?? "",
                            price = course.Price,
                            discountPrice = course.DiscountPrice,
                            discountEndDate = course.DiscountEndDate,
                            hasActiveDiscount = hasActiveDiscount,
                            effectivePrice = effectivePrice,
                            discountAmount = discountAmount,
                            discountPercent = discountPercent,
                            isEnrolled = enrolledIds.Contains(course.Id)   // ← جديد
                        };
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    id = teacher.Id,
                    name = teacher.Name,
                    subject = teacher.Subject,
                    bio = teacher.Bio,
                    image = teacher.Image,
                    courses = coursesForTeacher,
                    teacherCourses = coursesForTeacher
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting teacher courses for teacher ID: {TeacherId}", id);
                return Json(new { success = false, message = "حدث خطأ في تحميل بيانات المعلم" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetGradeCourses(int grade)
        {
            try
            {
                // 1. جلب أساتذة المرحلة
                var gradeTeachers = await _assignmentService.GetOrganizedGradesTeachers();
                var gradeData = gradeTeachers?.FirstOrDefault(g => g.Grade == grade);
                var teacherIds = gradeData?.Teachers?.Select(t => t.Id).ToList() ?? new List<int>();

                if (!teacherIds.Any())
                    return Json(new { success = true, courses = new List<object>(), total = 0 });

                // 2. جلب كل روابط (TeacherCourse) لهؤلاء الأساتذة
                var teacherCourses = await _teacherCourseRepository
                    .GetByKey(tc => teacherIds.Contains(tc.TeacherId) && tc.DeletedAt == null);

                // 3. استخراج المواد الفريدة (تجميع حسب CourseId)
                var courseIds = teacherCourses
                    .Where(tc => tc.CourseId > 0)
                    .Select(tc => tc.CourseId)
                    .Distinct()
                    .ToList();

                if (!courseIds.Any())
                    return Json(new { success = true, courses = new List<object>(), total = 0 });

                // 4. جلب بيانات كل مادة
                var allCourses = await _courseService.GetAvailableCourses();
                var coursesInGrade = allCourses.Where(c => courseIds.Contains(c.Id)).ToList();

                // 5. جلب مواد المستخدم المشترك بها
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var enrolledIds = new HashSet<int>();
                if (!string.IsNullOrEmpty(userId))
                {
                    try
                    {
                        var enrollments = await _courseService.GetUserEnrolledCourses(userId);
                        enrolledIds = enrollments.Select(e => e.CourseId).Distinct().ToHashSet();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to load enrollments for user {UserId}", userId);
                    }
                }

                // 6. تحضير النتيجة
                var result = coursesInGrade.Select(c => new
                {
                    id = c.Id,
                    name = c.Name ?? "",
                    description = c.Description ?? "",
                    price = c.Price,
                    discountPrice = c.DiscountPrice,
                    discountEndDate = c.DiscountEndDate,
                    hasActiveDiscount = c.HasActiveDiscount,
                    effectivePrice = c.EffectivePrice,
                    discountAmount = c.DiscountAmount,
                    discountPercent = c.DiscountPercent,
                    isEnrolled = enrolledIds.Contains(c.Id)
                }).ToList();

                return Json(new { success = true, courses = result, total = result.Count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting courses for grade {Grade}", grade);
                return Json(new { success = false, message = "حدث خطأ في تحميل المواد" });
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
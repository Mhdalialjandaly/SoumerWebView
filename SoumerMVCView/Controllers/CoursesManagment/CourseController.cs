using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoumerMVCView.Models;
using SoumerMVCView.Services.BalanceService;
using SoumerMVCView.Services.CourseService;
using System.Security.Claims;

namespace SoumerMVCView.Controllers.CoursesManagment
{
    [Authorize]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly IBalanceService _balanceService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            ICourseService courseService,
            IBalanceService balanceService,
            ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _balanceService = balanceService;
            _logger = logger;
        }

        // عرض جميع الكورسات
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var courses = await _courseService.GetAvailableCourses();
            var userBalance = await _balanceService.GetUserBalance(userId);
            var enrolledCourses = await _courseService.GetUserEnrolledCourses(userId);

            var model = new CourseListViewModel
            {
                Courses = courses,
                UserBalance = userBalance.CurrentBalance,
                TotalCourses = courses.Count,
                EnrolledCoursesCount = enrolledCourses.Count
            };

            return View(model);
        }

        // عرض تفاصيل الكورس
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var course = await _courseService.GetCourseById(id);

            if (course == null)
            {
                return NotFound();
            }

            var isEnrolled = await _courseService.IsUserEnrolled(id, userId);
            var userBalance = await _balanceService.GetUserBalance(userId);
            var eligibility = await _courseService.CheckEnrollmentEligibility(id, userId);

            var model = new CourseDetailViewModel
            {
                Course = course,
                IsEnrolled = isEnrolled,
                UserBalance = userBalance.CurrentBalance,
                CanEnroll = eligibility.IsEligible && !isEnrolled,
                EnrollmentMessage = eligibility.Message
            };

            return View(model);
        }

        // التسجيل في الكورس
        [HttpPost]
        public async Task<IActionResult> Enroll(int courseId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });
                }

                // التحقق من الأهلية
                var eligibility = await _courseService.CheckEnrollmentEligibility(courseId, userId);
                if (!eligibility.IsEligible)
                {
                    return Json(new { success = false, message = eligibility.Message });
                }

                // التسجيل في الكورس
                var result = await _courseService.EnrollInCourse(courseId, userId);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = $"تم التسجيل في الكورس بنجاح. تم خصم {eligibility.CoursePrice} نقطة من رصيدك"
                    });
                }

                return Json(new { success = false, message = "حدث خطأ أثناء التسجيل في الكورس" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enrolling in course {CourseId}", courseId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // إلغاء التسجيل من الكورس
        [HttpPost]
        public async Task<IActionResult> Unenroll(int courseId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });
                }

                var result = await _courseService.UnenrollFromCourse(courseId, userId);

                if (result)
                {
                    return Json(new { success = true, message = "تم إلغاء التسجيل من الكورس بنجاح" });
                }

                return Json(new { success = false, message = "حدث خطأ أثناء إلغاء التسجيل" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unenrolling from course {CourseId}", courseId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // عرض الكورسات المسجل فيها المستخدم
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

            return View(model);
        }

        // البحث عن الكورسات
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            var allCourses = await _courseService.GetAvailableCourses();
            var filteredCourses = string.IsNullOrEmpty(query)
                ? allCourses
                : allCourses.FindAll(c => c.Name.Contains(query) || c.Description.Contains(query));

            return Json(new { success = true, courses = filteredCourses });
        }
    }
}
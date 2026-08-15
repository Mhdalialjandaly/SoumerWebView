using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Models;
using SoumerMVCView.Services.CourseService;
using System.Security.Claims;

namespace SoumerMVCView.Controllers.CoursesManagment
{
    [Authorize]
    public class MyCoursesController : Controller
    {
        private readonly ILogger<MyCoursesController> _logger;
        private readonly ICourseService _courseService;
        private readonly ICourseVideoService _courseVideoService;
        private readonly ITeacherCourseRepository _teacherCourseRepository;

        public MyCoursesController(
            ILogger<MyCoursesController> logger,
            ICourseService courseService,
            ICourseVideoService courseVideoService,
            ITeacherCourseRepository teacherCourseRepository)
        {
            _logger = logger;
            _courseService = courseService;
            _courseVideoService = courseVideoService;
            _teacherCourseRepository = teacherCourseRepository;
        }

        // عرض صفحة كورساتي
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var enrolledCourses = await _courseService.GetUserEnrolledCourses(userId);

            // جلب الفيديوهات لكل كورس مسجل فيه المستخدم
            var courseVideos = new Dictionary<int, List<CourseVideoDto>>();
            var courseVideosCount = new Dictionary<int, int>();

            foreach (var enrollment in enrolledCourses)
            {
                var videos = await _courseVideoService.GetCourseVideos(enrollment.CourseId);
                courseVideos[enrollment.CourseId] = videos;
                courseVideosCount[enrollment.CourseId] = videos.Count(v => v.IsPublished);
            }

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
                TotalSpent = totalSpent,
                CourseVideos = courseVideos,
                CourseVideosCount = courseVideosCount
            };

            return View(model);
        }

        // الحصول على فيديوهات كورس معين
        [HttpGet]
        public async Task<IActionResult> GetCourseVideos(int courseId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });
                }

                // التحقق من أن المستخدم مسجل في الكورس
                var isEnrolled = await _courseService.IsUserEnrolled(courseId, userId);
                if (!isEnrolled)
                {
                    return Json(new { success = false, message = "يجب التسجيل في الكورس أولاً لمشاهدة الفيديوهات" });
                }

                var videos = await _courseVideoService.GetCourseVideos(courseId);
                var publishedVideos = videos.Where(v => v.IsPublished).ToList();

                return Json(new { success = true, videos = publishedVideos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting videos for course {CourseId}", courseId);
                return Json(new { success = false, message = "حدث خطأ في تحميل الفيديوهات" });
            }
        }

        // الحصول على تفاصيل فيديو معين
        [HttpGet]
        public async Task<IActionResult> GetVideoDetails(int videoId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });
                }

                var video = await _courseVideoService.GetVideoById(videoId);
                if (video == null)
                {
                    return Json(new { success = false, message = "الفيديو غير موجود" });
                }

                // التحقق من صلاحية المشاهدة
                var canWatch = await _courseVideoService.CanWatchVideo(videoId, userId);
                if (!canWatch)
                {
                    return Json(new { success = false, message = "لا يمكنك مشاهدة هذا الفيديو. يرجى التسجيل في الكورس أولاً" });
                }

                return Json(new { success = true, video });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video details {VideoId}", videoId);
                return Json(new { success = false, message = "حدث خطأ في تحميل الفيديو" });
            }
        }
    }
}
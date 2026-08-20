using Core.Enums;
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
            var totalVideos = 0;

            foreach (var enrollment in enrolledCourses)
            {
                var videos = await _courseVideoService.GetCourseVideos(enrollment.CourseId);

                // بناء EmbedUrl لكل فيديو
                foreach (var video in videos)
                {
                    if (string.IsNullOrEmpty(video.EmbedUrl))
                    {
                        video.EmbedUrl = BuildEmbedUrl(video);
                    }
                }

                courseVideos[enrollment.CourseId] = videos;
                courseVideosCount[enrollment.CourseId] = videos.Count(v => v.IsPublished);
                totalVideos += videos.Count(v => v.IsPublished);
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
                TotalVideos = totalVideos,
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

                // بناء EmbedUrl
                var embedUrl = BuildEmbedUrl(video);

                // إرجاع البيانات مع EmbedUrl
                return Json(new
                {
                    success = true,
                    video = new
                    {
                        id = video.Id,
                        title = video.Title,
                        description = video.Description,
                        videoUrl = video.VideoUrl,
                        videoId = video.VideoId,
                        embedUrl = embedUrl,
                        platform = (int)video.Platform,
                        duration = video.Duration,
                        courseId = video.CourseId,
                        courseName = video.CourseName,
                        isFree = video.IsFree,
                        isPublished = video.IsPublished,
                        createdAt = video.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video details {VideoId}", videoId);
                return Json(new { success = false, message = "حدث خطأ في تحميل الفيديو" });
            }
        }

        private string BuildEmbedUrl(CourseVideoDto video)
        {
            if (video == null) return null;

            // إذا كان EmbedUrl موجوداً مسبقاً، استخدمه
            if (!string.IsNullOrEmpty(video.EmbedUrl))
            {
                return video.EmbedUrl;
            }

            // استخدام VideoId إذا كان موجوداً
            string videoId = video.VideoId;

            // إذا لم يكن VideoId موجوداً، استخرجه من الرابط
            if (string.IsNullOrEmpty(videoId) && !string.IsNullOrEmpty(video.VideoUrl))
            {
                videoId = ExtractVideoIdFromUrl(video.VideoUrl, video.Platform);
            }

            // بناء الرابط حسب المنصة
            return video.Platform switch
            {
                VideoPlatform.YouTube => $"https://www.youtube.com/embed/{videoId}",
                VideoPlatform.Vimeo => $"https://player.vimeo.com/video/{videoId}",
                VideoPlatform.GoogleDrive => $"https://drive.google.com/file/d/{videoId}/preview",
                _ => video.VideoUrl
            };
        }

        private string ExtractVideoIdFromUrl(string url, VideoPlatform platform)
        {
            if (string.IsNullOrEmpty(url)) return null;

            switch (platform)
            {
                case VideoPlatform.YouTube:
                    var ytPatterns = new[]
                    {
                @"(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/|youtube\.com\/shorts\/)([^&\n?#/]+)",
                @"youtube\.com\/watch\?.*v=([^&\n?#]+)"
            };

                    foreach (var pattern in ytPatterns)
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(url, pattern);
                        if (match.Success && match.Groups.Count > 1)
                        {
                            return match.Groups[1].Value;
                        }
                    }
                    break;

                case VideoPlatform.Vimeo:
                    var vimeoMatch = System.Text.RegularExpressions.Regex.Match(url, @"vimeo\.com\/(\d+)");
                    if (vimeoMatch.Success)
                    {
                        return vimeoMatch.Groups[1].Value;
                    }
                    break;

                case VideoPlatform.GoogleDrive:
                    var driveMatch = System.Text.RegularExpressions.Regex.Match(url, @"\/d\/([^\/]+)");
                    if (driveMatch.Success)
                    {
                        return driveMatch.Groups[1].Value;
                    }
                    break;
            }

            return null;
        }
        [HttpGet]
        public async Task<IActionResult> WatchVideo(int videoId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login", "Account");
                }

                var video = await _courseVideoService.GetVideoById(videoId);
                if (video == null)
                {
                    return NotFound();
                }

                // التحقق من صلاحية المشاهدة
                var canWatch = await _courseVideoService.CanWatchVideo(videoId, userId);
                if (!canWatch)
                {
                    TempData["Error"] = "لا يمكنك مشاهدة هذا الفيديو. يرجى التسجيل في الكورس أولاً";
                    return RedirectToAction("Index");
                }

                // بناء EmbedUrl إذا لم يكن موجوداً
                if (string.IsNullOrEmpty(video.EmbedUrl))
                {
                    video.EmbedUrl = BuildEmbedUrl(video);
                }

                // جلب الفيديوهات المرتبطة
                var relatedVideos = await _courseVideoService.GetCourseVideos(video.CourseId);
                video.RelatedVideos = relatedVideos.Where(v => v.IsPublished).ToList();

                return View(video);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error watching video {VideoId}", videoId);
                TempData["Error"] = "حدث خطأ في تحميل الفيديو";
                return RedirectToAction("Index");
            }
        }
    }
}
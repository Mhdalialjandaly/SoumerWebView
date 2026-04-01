using Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
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
        private readonly ICourseVideoService _courseVideoService;
        private readonly ILogger<CourseController> _logger;

        public CourseController(
            ICourseService courseService,
            IBalanceService balanceService,
            ILogger<CourseController> logger,
            ICourseVideoService courseVideoService)
        {
            _courseService = courseService;
            _balanceService = balanceService;
            _logger = logger;
            _courseVideoService = courseVideoService;
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
        public async Task<IActionResult> CourseVideos(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // التحقق من أن المستخدم مسجل في الكورس
            var isEnrolled = await _courseService.IsUserEnrolled(courseId, userId);
            if (!isEnrolled)
            {
                TempData["Error"] = "يجب التسجيل في الكورس أولاً لمشاهدة الفيديوهات";
                return RedirectToAction("Details", new { id = courseId });
            }

            // جلب معلومات الكورس
            var course = await _courseService.GetCourseById(courseId);
            if (course == null)
            {
                return NotFound();
            }

            // جلب فيديوهات الكورس
            var videos = await _courseVideoService.GetCourseVideos(courseId);
            var publishedVideos = videos.Where(v => v.IsPublished).OrderBy(v => v.Order).ToList();

            var model = new CourseVideosViewModel
            {
                CourseId = courseId,
                CourseName = course.Name,
                CourseDescription = course.Description,
                Videos = publishedVideos,
                IsEnrolled = true,
                TotalVideos = publishedVideos.Count
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> TrackVideoView(int videoId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false });
                }

                // يمكن إضافة تتبع المشاهدة هنا
                // await _videoTrackingService.TrackView(videoId, userId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error tracking video view");
                return Json(new { success = false });
            }
        }

        // حفظ تقدم المشاهدة
        [HttpPost]
        public async Task<IActionResult> SaveVideoProgress([FromBody] VideoProgressModel progress)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false });
                }

                // حفظ تقدم المشاهدة
                // await _videoProgressService.SaveProgress(progress.VideoId, userId, progress.Progress, progress.WatchTime);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving video progress");
                return Json(new { success = false });
            }
        }

        // الإبلاغ عن مشكلة في الفيديو
        [HttpPost]
        public async Task<IActionResult> ReportVideoIssue([FromBody] VideoReportModel report)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "يرجى تسجيل الدخول" });
                }

                // حفظ البلاغ
                // await _videoReportService.ReportIssue(report.VideoId, userId, report.Message);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting video issue");
                return Json(new { success = false, message = "حدث خطأ في إرسال البلاغ" });
            }
        }

        // الحصول على فيديوهات الكورس (للعرض في المودال)
        [HttpGet]
        public async Task<IActionResult> GetCourseVideos(int courseId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "يرجى تسجيل الدخول أولاً"
                    });
                }

                // التحقق من أن المستخدم مسجل في الكورس
                var isEnrolled = await _courseService.IsUserEnrolled(courseId, userId);
                if (!isEnrolled)
                {
                    return Json(new
                    {
                        success = false,
                        message = "يجب التسجيل في الكورس أولاً لمشاهدة الفيديوهات"
                    });
                }

                // جلب فيديوهات الكورس
                var videos = await _courseVideoService.GetCourseVideos(courseId);

                // فلترة الفيديوهات المنشورة فقط
                var publishedVideos = videos
                    .Where(v => v.IsPublished)
                    .OrderBy(v => v.Order)
                    .Select(v => new
                    {
                        id = v.Id,
                        title = v.Title,
                        description = v.Description,
                        videoUrl = v.VideoUrl,
                        platform = (int)v.Platform,
                        videoId = v.VideoId,
                        embedUrl = v.EmbedUrl,
                        duration = v.Duration,
                        order = v.Order,
                        isFree = v.IsFree,
                        isPublished = v.IsPublished,
                        thumbnailUrl = v.Platform == VideoPlatform.YouTube
                            ? $"https://img.youtube.com/vi/{v.VideoId}/mqdefault.jpg"
                            : (v.Platform == VideoPlatform.Vimeo
                                ? $"https://vumbnail.com/{v.VideoId}.jpg"
                                : "/images/video-placeholder.jpg")
                    })
                    .ToList();

                if (!publishedVideos.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "لا توجد فيديوهات منشورة في هذا الكورس حالياً"
                    });
                }

                return Json(new
                {
                    success = true,
                    videos = publishedVideos,
                    totalVideos = publishedVideos.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting videos for course {CourseId}", courseId);
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ في تحميل الفيديوهات. يرجى المحاولة مرة أخرى"
                });
            }
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddVideo(AddVideoViewModel model)
        {
            try
            {
                // التحقق من صحة الرابط
                var videoInfo = await _courseVideoService.ExtractVideoInfo(model.VideoUrl);
                if (string.IsNullOrEmpty(videoInfo.VideoId))
                {
                    return Json(new { success = false, message = "رابط الفيديو غير صحيح" });
                }

                var videoDto = new CourseVideoDto
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Description = model.Description,
                    VideoUrl = model.VideoUrl, // حفظ الرابط الأصلي
                    Platform = videoInfo.Platform,
                    VideoId = videoInfo.VideoId,
                    EmbedUrl = videoInfo.EmbedUrl,
                    Order = model.Order,
                    IsFree = model.IsFree,
                    IsPublished = model.IsPublished,
                    CreatedAt = DateTime.Now
                };

                var result = await _courseVideoService.AddVideo(videoDto);

                if (result)
                {
                    return Json(new { success = true, message = "تم إضافة الفيديو بنجاح" });
                }

                return Json(new { success = false, message = "حدث خطأ أثناء إضافة الفيديو" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding video");
                return Json(new { success = false, message = "حدث خطأ في إضافة الفيديو" });
            }
        }
        // الحصول على تفاصيل فيديو معين للتشغيل
        [HttpGet]
        public async Task<IActionResult> GetVideoDetails(int videoId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new
                    {
                        success = false,
                        message = "يرجى تسجيل الدخول أولاً"
                    });
                }

                // جلب تفاصيل الفيديو
                var video = await _courseVideoService.GetVideoById(videoId);
                if (video == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "الفيديو غير موجود"
                    });
                }

                // التحقق من أن الفيديو منشور
                if (!video.IsPublished)
                {
                    return Json(new
                    {
                        success = false,
                        message = "هذا الفيديو غير متاح حالياً"
                    });
                }

                // التحقق من صلاحية المشاهدة
                var canWatch = await _courseVideoService.CanWatchVideo(videoId, userId);
                if (!canWatch)
                {
                    return Json(new
                    {
                        success = false,
                        message = "لا يمكنك مشاهدة هذا الفيديو. يرجى التسجيل في الكورس أولاً"
                    });
                }

                // تحضير بيانات الفيديو للإرسال
                var videoData = new
                {
                    id = video.Id,
                    title = video.Title,
                    description = video.Description,
                    videoUrl = video.VideoUrl,
                    platform = (int)video.Platform,
                    videoId = video.VideoId,
                    embedUrl = video.EmbedUrl,
                    duration = video.Duration,
                    order = video.Order,
                    isFree = video.IsFree,
                    courseId = video.CourseId,
                    courseName = video.CourseName
                };

                // تسجيل مشاهدة الفيديو (اختياري)
                await TrackVideoView(videoId, userId);

                return Json(new
                {
                    success = true,
                    video = videoData
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting video details {VideoId}", videoId);
                return Json(new
                {
                    success = false,
                    message = "حدث خطأ في تحميل الفيديو. يرجى المحاولة مرة أخرى"
                });
            }
        }
        // تتبع مشاهدة الفيديو
        private async Task TrackVideoView(int videoId, string userId)
        {
            try
            {
                // يمكن إضافة تتبع المشاهدة هنا
                // مثلاً: حفظ في جدول VideoViews
                /*
                var videoView = new VideoView
                {
                    VideoId = videoId,
                    UserId = userId,
                    ViewedAt = DateTime.Now,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
                };
                await _videoViewRepository.Add(videoView);
                */

                // تحديث عدد المشاهدات للفيديو
                // await _courseVideoService.IncrementViewCount(videoId);
            }
            catch (Exception ex)
            {
                // لا نريد إظهار الخطأ للمستخدم، فقط تسجيله
                _logger.LogWarning(ex, "Error tracking video view for video {VideoId}", videoId);
            }
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

        // إضافة دالة للتحقق من صحة الرابط
        public async Task<bool> ValidateVideoUrl(string videoUrl)
        {
            try
            {
                var videoInfo = await _courseVideoService.ExtractVideoInfo(videoUrl);
                return !string.IsNullOrEmpty(videoInfo.VideoId);
            }
            catch
            {
                return false;
            }
        }
    }
}
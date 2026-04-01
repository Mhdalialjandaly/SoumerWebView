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
        private readonly ITeacherCourseRepository _teacherCourseRepository; 
        private readonly ICourseVideoService _courseVideoService;
        public HomeController(
            ILogger<HomeController> logger,
            ITeacherRepository teacherRepository,
            ITeacherAssignmentService assignmentService,
            ICourseService courseService,
            IBalanceService balanceService,
            ITeacherCourseRepository teacherCourseRepository,
            ICourseVideoService courseVideoService)
        {
            _logger = logger;
            _teacherRepository = teacherRepository;
            _assignmentService = assignmentService;
            _courseService = courseService;
            _balanceService = balanceService;
            _teacherCourseRepository = teacherCourseRepository;
            _courseVideoService = courseVideoService;
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

        public async Task<IActionResult> MyCourses()
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

            return PartialView("_MyCourses", model);
        }
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


        [HttpGet]
        public async Task<IActionResult> GetTeacherCourses(int id)
        {
            try
            {
                // جلب بيانات الأستاذ
                var teacher = await _teacherRepository.GetById(id);
                if (teacher == null)
                {
                    return Json(new { success = false, message = "الأستاذ غير موجود" });
                }

                // جلب الكورسات المرتبطة بهذا الأستاذ
                var teacherCourses = await _teacherCourseRepository.GetTeacherCoursesWithDetails(id); 

                // فلترة الكورسات الخاصة بهذا الأستاذ
                var coursesForTeacher = teacherCourses
                    .Select(tc => new
                    {
                        id = tc.CourseId,
                        name = tc.Course?.Name ?? "",
                        description = tc.Course?.Description ?? "",
                        price = tc.Course?.Price ?? 0
                    })
                    .ToList();

                // تحضير البيانات للإرسال
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
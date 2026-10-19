using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Services.ExamService;
using System.Security.Claims;

namespace SoumerMVCView.Controllers.CoursesManagment
{
    [Authorize]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly ILogger<ExamController> _logger;

        public ExamController(IExamService examService, ILogger<ExamController> logger)
        {
            _examService = examService;
            _logger = logger;
        }

        // ==========================================
        // صفحة قائمة امتحانات المادة
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index(int courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var exams = await _examService.GetExamsForCourse(courseId, userId);
            ViewBag.CourseId = courseId;
            return View(exams);
        }

        // ==========================================
        // بدء الامتحان
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int examId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "يرجى تسجيل الدخول" });

            try
            {
                var session = await _examService.StartExam(examId, userId);
                return Json(new
                {
                    success = true,
                    attemptId = session.AttemptId,
                    redirectUrl = Url.Action(nameof(Take), new { attemptId = session.AttemptId })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // واجهة الامتحان
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Take(int attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var session = await _examService.GetActiveSession(attemptId, userId);
            if (session == null)
            {
                // ربما انتهت الجلسة أو تم التسليم
                TempData["Error"] = "الجلسة منتهية أو غير صالحة";
                return RedirectToAction(nameof(Result), new { attemptId });
            }

            return View(session);
        }

        // ==========================================
        // حفظ إجابة سؤال
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Answer([FromBody] SubmitAnswerDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "يرجى تسجيل الدخول" });

            var result = await _examService.SubmitAnswer(dto, userId);
            return Json(result);
        }

        // ==========================================
        // تسليم الامتحان يدوياً
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "يرجى تسجيل الدخول" });

            try
            {
                var result = await _examService.SubmitExam(attemptId, userId, "ManualSubmit");
                return Json(new
                {
                    success = true,
                    resultUrl = Url.Action(nameof(Result), new { attemptId })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // مغادرة الصفحة (ForcedFail)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceFail(int attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            try
            {
                await _examService.SubmitExam(attemptId, userId, "ForcedFail");
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error force-failing attempt {AttemptId}", attemptId);
                return Json(new { success = false });
            }
        }

        // ==========================================
        // انتهاء الوقت (TimeExpired)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TimeExpired(int attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false });

            try
            {
                var result = await _examService.SubmitExam(attemptId, userId, "TimeExpired");
                return Json(new
                {
                    success = true,
                    resultUrl = Url.Action(nameof(Result), new { attemptId })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==========================================
        // صفحة النتيجة
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Result(int attemptId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var result = await _examService.GetResult(attemptId, userId);
            if (result == null)
                return NotFound();

            return View(result);
        }
    }
}
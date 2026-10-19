using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SoumerMVCView.Controllers.Managment
{
    [Authorize(Roles = "Admin")]
    public class AdminExamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminExamController> _logger;

        public AdminExamController(ApplicationDbContext context, ILogger<AdminExamController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ==========================================
        // قائمة الامتحانات
        // ==========================================
        public async Task<IActionResult> Index(int? courseId)
        {
            var query = _context.Set<Exam>()
                .Include(e => e.Course)
                .Include(e => e.Questions)
                .Where(e => e.DeletedAt == null);

            if (courseId.HasValue && courseId.Value > 0)
                query = query.Where(e => e.CourseId == courseId.Value);

            var exams = await query
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
            ViewBag.SelectedCourseId = courseId;

            return View(exams);
        }

        // ==========================================
        // إنشاء امتحان
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .OrderBy(c => c.Name)
                .ToListAsync();
            return View(new Exam());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Exam exam)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.DeletedAt == null).OrderBy(c => c.Name).ToListAsync();
                return View(exam);
            }

            exam.CreatedAt = DateTime.Now;
            _context.Set<Exam>().Add(exam);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم إنشاء الامتحان. الآن أضف الأسئلة.";
            return RedirectToAction(nameof(Edit), new { id = exam.Id });
        }

        // ==========================================
        // تعديل امتحان + إدارة الأسئلة
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var exam = await _context.Set<Exam>()
                .Include(e => e.Course)
                .Include(e => e.Questions.Where(q => q.DeletedAt == null))
                    .ThenInclude(q => q.Options.Where(o => o.DeletedAt == null))
                .FirstOrDefaultAsync(e => e.Id == id && e.DeletedAt == null);

            if (exam == null) return NotFound();

            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null).OrderBy(c => c.Name).ToListAsync();

            return View(exam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Exam exam)
        {
            if (id != exam.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.DeletedAt == null).OrderBy(c => c.Name).ToListAsync();
                return View(exam);
            }

            var existing = await _context.Set<Exam>().FindAsync(id);
            if (existing == null) return NotFound();

            existing.CourseId = exam.CourseId;
            existing.Title = exam.Title;
            existing.Description = exam.Description;
            existing.DurationMinutes = exam.DurationMinutes;
            existing.PassingScore = exam.PassingScore;
            existing.AllowRetake = exam.AllowRetake;
            existing.MaxAttempts = exam.MaxAttempts;
            existing.ShuffleQuestions = exam.ShuffleQuestions;
            existing.IsPublished = exam.IsPublished;
            existing.ModifiedAt = DateTime.Now;
            existing.ModifiedBy = User.Identity?.Name;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم تحديث الامتحان";
            return RedirectToAction(nameof(Edit), new { id });
        }

        // ==========================================
        // إضافة سؤال
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(int examId, string questionText, int points)
        {
            try
            {
                var exam = await _context.Set<Exam>().FindAsync(examId);
                if (exam == null) return Json(new { success = false, message = "الامتحان غير موجود" });

                var order = await _context.Set<ExamQuestion>()
                    .Where(q => q.ExamId == examId && q.DeletedAt == null)
                    .CountAsync() + 1;

                var question = new ExamQuestion
                {
                    ExamId = examId,
                    QuestionText = questionText,
                    Points = points,
                    Order = order,
                    CreatedAt = DateTime.Now
                };

                _context.Set<ExamQuestion>().Add(question);
                await _context.SaveChangesAsync();

                return Json(new { success = true, questionId = question.Id, order = question.Order });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding question");
                return Json(new { success = false, message = "حدث خطأ" });
            }
        }

        // ==========================================
        // إضافة خيار
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOption(int questionId, string optionText, bool isCorrect)
        {
            try
            {
                var question = await _context.Set<ExamQuestion>().FindAsync(questionId);
                if (question == null) return Json(new { success = false, message = "السؤال غير موجود" });

                var order = await _context.Set<ExamOption>()
                    .Where(o => o.ExamQuestionId == questionId && o.DeletedAt == null)
                    .CountAsync() + 1;

                var option = new ExamOption
                {
                    ExamQuestionId = questionId,
                    OptionText = optionText,
                    IsCorrect = isCorrect,
                    Order = order,
                    CreatedAt = DateTime.Now
                };

                _context.Set<ExamOption>().Add(option);
                await _context.SaveChangesAsync();

                return Json(new { success = true, optionId = option.Id, order = option.Order });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding option");
                return Json(new { success = false, message = "حدث خطأ" });
            }
        }

        // ==========================================
        // حذف سؤال/خيار
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuestion(int id)
        {
            var q = await _context.Set<ExamQuestion>().FindAsync(id);
            if (q == null) return Json(new { success = false });

            q.DeletedAt = DateTime.Now;
            q.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var o = await _context.Set<ExamOption>().FindAsync(id);
            if (o == null) return Json(new { success = false });

            o.DeletedAt = DateTime.Now;
            o.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ==========================================
        // حذف الامتحان
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var exam = await _context.Set<Exam>().FindAsync(id);
            if (exam == null) return NotFound();

            exam.DeletedAt = DateTime.Now;
            exam.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الامتحان";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // محاولات الطلاب
        // ==========================================
        public async Task<IActionResult> Attempts(int examId)
        {
            var exam = await _context.Set<Exam>()
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.Id == examId && e.DeletedAt == null);

            if (exam == null) return NotFound();

            var attempts = await _context.Set<ExamAttempt>()
                .Include(a => a.User)
                .Where(a => a.ExamId == examId && a.DeletedAt == null)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();

            ViewBag.Exam = exam;
            return View(attempts);
        }
    }
}
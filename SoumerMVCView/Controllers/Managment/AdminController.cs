using Core.Enums;
using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SoumerMVCView.Models;
using SoumerMVCView.Services.BalanceService;

namespace SoumerMVCView.Controllers.Managment
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBalanceService _balanceService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long _maxFileSize = 5 * 1024 * 1024; // 5MB
        public AdminController(ApplicationDbContext context,
            IBalanceService balanceService,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _balanceService = balanceService;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==================== DASHBOARD ====================
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                TotalTeachers = await _context.Teachers.CountAsync(t => t.DeletedAt == null),
                TotalCourses = await _context.Courses.CountAsync(c => c.DeletedAt == null),
                TotalVideos = await _context.CourseVideos.CountAsync(v => v.DeletedAt == null),
                TotalTeacherCourses = await _context.TeacherCourses.CountAsync(tc => tc.DeletedAt == null),
                RecentTeachers = await _context.Teachers
                    .Where(t => t.DeletedAt == null)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(5)
                    .ToListAsync(),
                RecentCourses = await _context.Courses
                    .Where(c => c.DeletedAt == null)
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
        // ==================== TEACHER GRADE ASSIGNMENTS ====================
        public async Task<IActionResult> TeacherGrades()
        {
            var assignments = await _context.Set<TeacherGradeAssignment>()
                .Include(a => a.Teacher)
                .Where(a => a.DeletedAt == null)
                .OrderBy(a => a.Grade)
                .ThenBy(a => a.Priority)
                .ToListAsync();

            // تجميع حسب الصف
            var groupedByGrade = assignments
                .GroupBy(a => a.Grade)
                .OrderBy(g => g.Key)
                .ToList();

            ViewBag.GroupedAssignments = groupedByGrade;
            ViewBag.TotalTeachers = await _context.Teachers.CountAsync(t => t.DeletedAt == null);
            ViewBag.AssignedTeachers = assignments.Select(a => a.TeacherId).Distinct().Count();
            ViewBag.GradesWithTeachers = assignments.Select(a => a.Grade).Distinct().Count();

            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> AssignTeacherToGrade()
        {
            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.DeletedAt == null)
                .OrderBy(t => t.Name)
                .ToListAsync();

            // قائمة الصفوف المتاحة
            ViewBag.Grades = new List<SelectListItem>
            {
                new SelectListItem("الصف الأول", "1"),
                new SelectListItem("الصف الثاني", "2"),
                new SelectListItem("الصف الثالث", "3"),
                new SelectListItem("الصف الرابع", "4"),
                new SelectListItem("الصف الخامس", "5"),
                new SelectListItem("الصف السادس", "6"),
                new SelectListItem("الصف السابع", "7"),
                new SelectListItem("الصف الثامن", "8"),
                new SelectListItem("الصف التاسع", "9"),
                new SelectListItem("الصف العاشر", "10"),
                new SelectListItem("الصف الحادي عشر", "11"),
                new SelectListItem("الصف الثاني عشر", "12")
            };

            return View(new TeacherGradeAssignment());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacherToGrade(TeacherGradeAssignment assignment)
        {
            if (ModelState.IsValid)
            {
                // التحقق من عدم وجود تعيين مسبق
                var exists = await _context.Set<TeacherGradeAssignment>()
                    .AnyAsync(a => a.TeacherId == assignment.TeacherId
                                && a.Grade == assignment.Grade
                                && a.DeletedAt == null);

                if (exists)
                {
                    TempData["ErrorMessage"] = "هذا الأستاذ معين مسبقاً لهذا الصف";
                    ViewBag.Teachers = await _context.Teachers
                        .Where(t => t.DeletedAt == null)
                        .OrderBy(t => t.Name)
                        .ToListAsync();
                    ViewBag.Grades = GetGradesList();
                    return View(assignment);
                }

                assignment.CreatedAt = DateTime.Now;
                _context.Set<TeacherGradeAssignment>().Add(assignment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم تعيين الأستاذ للصف بنجاح";
                return RedirectToAction(nameof(TeacherGrades));
            }

            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.DeletedAt == null)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.Grades = GetGradesList();
            return View(assignment);
        }

        [HttpGet]
        public async Task<IActionResult> EditTeacherGrade(int id)
        {
            var assignment = await _context.Set<TeacherGradeAssignment>()
                .Include(a => a.Teacher)
                .FirstOrDefaultAsync(a => a.Id == id && a.DeletedAt == null);

            if (assignment == null)
                return NotFound();

            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.DeletedAt == null)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.Grades = GetGradesList();

            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacherGrade(int id, TeacherGradeAssignment assignment)
        {
            if (id != assignment.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    assignment.ModifiedAt = DateTime.Now;
                    _context.Set<TeacherGradeAssignment>().Update(assignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Set<TeacherGradeAssignment>().AnyAsync(a => a.Id == id))
                        return NotFound();
                    else
                        throw;
                }

                TempData["SuccessMessage"] = "تم تحديث تعيين الأستاذ بنجاح";
                return RedirectToAction(nameof(TeacherGrades));
            }

            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.DeletedAt == null)
                .OrderBy(t => t.Name)
                .ToListAsync();
            ViewBag.Grades = GetGradesList();
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacherGrade(int id)
        {
            var assignment = await _context.Set<TeacherGradeAssignment>().FindAsync(id);
            if (assignment == null)
                return NotFound();

            assignment.DeletedAt = DateTime.Now;
            assignment.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف تعيين الأستاذ من الصف بنجاح";
            return RedirectToAction(nameof(TeacherGrades));
        }

        // تعيين أساتذة متعددين لصف معين
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkAssignTeachers(int grade, int[] teacherIds, int[] priorities)
        {
            if (teacherIds == null || teacherIds.Length == 0)
            {
                TempData["ErrorMessage"] = "يرجى اختيار أستاذ واحد على الأقل";
                return RedirectToAction(nameof(TeacherGrades));
            }

            var addedCount = 0;
            var skippedCount = 0;

            for (int i = 0; i < teacherIds.Length; i++)
            {
                var teacherId = teacherIds[i];
                var priority = priorities != null && i < priorities.Length ? priorities[i] : i + 1;

                // التحقق من عدم وجود تعيين مسبق
                var exists = await _context.Set<TeacherGradeAssignment>()
                    .AnyAsync(a => a.TeacherId == teacherId
                                && a.Grade == grade
                                && a.DeletedAt == null);

                if (!exists)
                {
                    var assignment = new TeacherGradeAssignment
                    {
                        TeacherId = teacherId,
                        Grade = grade,
                        Priority = priority,
                        CreatedAt = DateTime.Now
                    };
                    _context.Set<TeacherGradeAssignment>().Add(assignment);
                    addedCount++;
                }
                else
                {
                    skippedCount++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"تم تعيين {addedCount} أستاذ للصف {GetGradeName(grade)}" +
                                          (skippedCount > 0 ? $" (تم تخطي {skippedCount} تعيين مكرر)" : "");

            return RedirectToAction(nameof(TeacherGrades));
        }

        // API للحصول على أساتذة صف معين
        [HttpGet]
        public async Task<IActionResult> GetTeachersByGrade(int grade)
        {
            var teachers = await _context.Set<TeacherGradeAssignment>()
                .Include(a => a.Teacher)
                .Where(a => a.Grade == grade && a.DeletedAt == null)
                .OrderBy(a => a.Priority)
                .Select(a => new
                {
                    a.Id,
                    a.TeacherId,
                    TeacherName = a.Teacher.Name,
                    a.Teacher.Subject,
                    a.Priority,
                    a.Teacher.Image
                })
                .ToListAsync();

            return Json(new { success = true, teachers });
        }

        // API لإحصائيات التوزيع
        [HttpGet]
        public async Task<IActionResult> GetGradeDistributionStats()
        {
            var stats = await _context.Set<TeacherGradeAssignment>()
                .Where(a => a.DeletedAt == null)
                .GroupBy(a => a.Grade)
                .Select(g => new
                {
                    Grade = g.Key,
                    GradeName = GetGradeName(g.Key),
                    TeacherCount = g.Select(a => a.TeacherId).Distinct().Count(),
                    TotalAssignments = g.Count()
                })
                .OrderBy(s => s.Grade)
                .ToListAsync();

            return Json(new { success = true, stats });
        }

        // ==================== TEACHERS CRUD ====================
        public async Task<IActionResult> Teachers()
        {
            var teachers = await _context.Teachers
                .Where(t => t.DeletedAt == null)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(teachers);
        }

        [HttpGet]
        public IActionResult CreateTeacher()
        {
            return View(new Teacher());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(Teacher teacher, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var imagePath = await UploadImage(ImageFile);
                    if (imagePath == null)
                    {
                        ModelState.AddModelError("ImageFile", "فشل في رفع الصورة. تأكد من صيغة وحجم الملف.");
                        return View(teacher);
                    }
                    teacher.Image = imagePath;
                }

                teacher.CreatedAt = DateTime.Now;
                teacher.Name = $"{teacher.FirstName} {teacher.LastName}";
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إضافة الأستاذ بنجاح";
                return RedirectToAction(nameof(Teachers));
            }

            return View(teacher);
        }

        // استبدل دالة EditTeacher بـ:
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(int id, Teacher teacher, IFormFile ImageFile)
        {
            if (id != teacher.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTeacher = await _context.Teachers.FindAsync(id);
                    if (existingTeacher == null)
                        return NotFound();

                    // Handle image upload
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingTeacher.Image))
                        {
                            DeleteImage(existingTeacher.Image);
                        }

                        var imagePath = await UploadImage(ImageFile);
                        if (imagePath == null)
                        {
                            ModelState.AddModelError("ImageFile", "فشل في رفع الصورة. تأكد من صيغة وحجم الملف.");
                            return View(teacher);
                        }
                        teacher.Image = imagePath;
                    }
                    else
                    {
                        // Keep existing image
                        teacher.Image = existingTeacher.Image;
                    }

                    teacher.Name = $"{teacher.FirstName} {teacher.LastName}";
                    teacher.ModifiedAt = DateTime.Now;
                    teacher.CreatedAt = existingTeacher.CreatedAt; // Preserve original creation date

                    _context.Entry(existingTeacher).CurrentValues.SetValues(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id))
                        return NotFound();
                    else
                        throw;
                }

                TempData["SuccessMessage"] = "تم تحديث بيانات الأستاذ بنجاح";
                return RedirectToAction(nameof(Teachers));
            }

            return View(teacher);
        }

        // دالة مساعدة لرفع الصورة
        private async Task<string> UploadImage(IFormFile file)
        {
            // Validate extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                return null;
            }

            // Validate size
            if (file.Length > _maxFileSize)
            {
                return null;
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";

            // Ensure uploads directory exists
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "teachers");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            return $"/uploads/teachers/{uniqueFileName}";
        }

        // دالة مساعدة لحذف الصورة القديمة
        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null || teacher.DeletedAt != null)
                return NotFound();

            return View(teacher);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditTeacher(int id, Teacher teacher)
        //{
        //    if (id != teacher.Id)
        //        return NotFound();

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            teacher.Name = $"{teacher.FirstName} {teacher.LastName}";
        //            teacher.ModifiedAt = DateTime.Now;
        //            _context.Update(teacher);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TeacherExists(teacher.Id))
        //                return NotFound();
        //            else
        //                throw;
        //        }

        //        TempData["SuccessMessage"] = "تم تحديث بيانات الأستاذ بنجاح";
        //        return RedirectToAction(nameof(Teachers));
        //    }

        //    return View(teacher);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
                return NotFound();

            // Soft delete
            teacher.DeletedAt = DateTime.Now;
            teacher.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الأستاذ بنجاح";
            return RedirectToAction(nameof(Teachers));
        }

        // ==================== COURSES CRUD ====================
        public async Task<IActionResult> Courses()
        {
            var courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View(new Course());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CreatedAt = DateTime.Now;
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إضافة الكورس بنجاح";
                return RedirectToAction(nameof(Courses));
            }

            return View(course);
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.DeletedAt != null)
                return NotFound();

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(int id, Course course)
        {
            if (id != course.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    course.ModifiedAt = DateTime.Now;
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.Id))
                        return NotFound();
                    else
                        throw;
                }

                TempData["SuccessMessage"] = "تم تحديث الكورس بنجاح";
                return RedirectToAction(nameof(Courses));
            }

            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            course.DeletedAt = DateTime.Now;
            course.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الكورس بنجاح";
            return RedirectToAction(nameof(Courses));
        }

        // ==================== COURSE VIDEOS CRUD ====================
        public async Task<IActionResult> CourseVideos()
        {
            var videos = await _context.CourseVideos
                .Include(v => v.Course)
                .Where(v => v.DeletedAt == null)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return View(videos);
        }

        [HttpGet]
        public async Task<IActionResult> CreateVideo()
        {
            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
            return View(new CourseVideo());
        }

        // إضافة دالة مساعدة للتحقق من صحة رابط الفيديو
        private bool ValidateVideoUrl(string url, VideoPlatform platform)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            switch (platform)
            {
                case VideoPlatform.YouTube:
                    return url.Contains("youtube.com") || url.Contains("youtu.be");

                case VideoPlatform.Vimeo:
                    return url.Contains("vimeo.com");

                case VideoPlatform.GoogleDrive:
                    return url.Contains("drive.google.com");

                default:
                    return true; // للمنصات الأخرى
            }
        }

        // استخراج معرف الفيديو (اختياري - للتخزين في قاعدة البيانات)
        private string ExtractVideoId(string url, VideoPlatform platform)
        {
            if (string.IsNullOrEmpty(url))
                return null;

            try
            {
                switch (platform)
                {
                    case VideoPlatform.YouTube:
                        var ytMatch = System.Text.RegularExpressions.Regex.Match(url,
                            @"(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([^&\n?#]+)");
                        return ytMatch.Success ? ytMatch.Groups[1].Value : null;

                    case VideoPlatform.Vimeo:
                        var vimeoMatch = System.Text.RegularExpressions.Regex.Match(url,
                            @"vimeo\.com\/(\d+)");
                        return vimeoMatch.Success ? vimeoMatch.Groups[1].Value : null;

                    case VideoPlatform.GoogleDrive:
                        var driveMatch = System.Text.RegularExpressions.Regex.Match(url,
                            @"\/d\/([^\/]+)");
                        return driveMatch.Success ? driveMatch.Groups[1].Value : null;

                    default:
                        return url;
                }
            }
            catch
            {
                return null;
            }
        }

        // تحديث CreateVideo لاستخراج معرف الفيديو تلقائياً
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVideo(CourseVideo video)
        {
            if (ModelState.IsValid)
            {
                // استخراج معرف الفيديو تلقائياً إذا لم يتم إدخاله
                if (string.IsNullOrEmpty(video.VideoId))
                {
                    video.VideoId = ExtractVideoId(video.VideoUrl, video.Platform);
                }

                // التحقق من صحة الرابط
                if (!ValidateVideoUrl(video.VideoUrl, video.Platform))
                {
                    ModelState.AddModelError("VideoUrl", "رابط الفيديو غير صالح للمنصة المختارة");
                    ViewBag.Courses = await _context.Courses
                        .Where(c => c.DeletedAt == null)
                        .ToListAsync();
                    return View(video);
                }

                video.CreatedAt = DateTime.Now;
                video.IsPublished = true;
                video.PublishDate = DateTime.Now;
                _context.CourseVideos.Add(video);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم إضافة الفيديو بنجاح";
                return RedirectToAction(nameof(CourseVideos));
            }

            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
            return View(video);
        }

        // تحديث EditVideo بنفس الطريقة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVideo(int id, CourseVideo video)
        {
            if (id != video.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // استخراج معرف الفيديو تلقائياً
                    if (string.IsNullOrEmpty(video.VideoId))
                    {
                        video.VideoId = ExtractVideoId(video.VideoUrl, video.Platform);
                    }

                    // التحقق من صحة الرابط
                    if (!ValidateVideoUrl(video.VideoUrl, video.Platform))
                    {
                        ModelState.AddModelError("VideoUrl", "رابط الفيديو غير صالح للمنصة المختارة");
                        ViewBag.Courses = await _context.Courses
                            .Where(c => c.DeletedAt == null)
                            .ToListAsync();
                        return View(video);
                    }

                    video.ModifiedAt = DateTime.Now;
                    _context.Update(video);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VideoExists(video.Id))
                        return NotFound();
                    else
                        throw;
                }

                TempData["SuccessMessage"] = "تم تحديث الفيديو بنجاح";
                return RedirectToAction(nameof(CourseVideos));
            }

            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
            return View(video);
        }
        [HttpGet]
        public async Task<IActionResult> EditVideo(int id)
        {
            var video = await _context.CourseVideos.FindAsync(id);
            if (video == null || video.DeletedAt != null)
                return NotFound();

            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
            return View(video);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> EditVideo(int id, CourseVideo video)
        //{
        //    if (id != video.Id)
        //        return NotFound();

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            video.ModifiedAt = DateTime.Now;
        //            _context.Update(video);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!VideoExists(video.Id))
        //                return NotFound();
        //            else
        //                throw;
        //        }

        //        TempData["SuccessMessage"] = "تم تحديث الفيديو بنجاح";
        //        return RedirectToAction(nameof(CourseVideos));
        //    }

        //    ViewBag.Courses = await _context.Courses
        //        .Where(c => c.DeletedAt == null)
        //        .ToListAsync();
        //    return View(video);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var video = await _context.CourseVideos.FindAsync(id);
            if (video == null)
                return NotFound();

            video.DeletedAt = DateTime.Now;
            video.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الفيديو بنجاح";
            return RedirectToAction(nameof(CourseVideos));
        }

        // ==================== TEACHER COURSES CRUD ====================
        public async Task<IActionResult> TeacherCourses()
        {
            var teacherCourses = await _context.TeacherCourses
                .Include(tc => tc.Teacher)
                .Include(tc => tc.Course)
                .Where(tc => tc.DeletedAt == null)
                .OrderByDescending(tc => tc.CreatedAt)
                .ToListAsync();

            return View(teacherCourses);
        }

        [HttpGet]
        public async Task<IActionResult> CreateTeacherCourse()
        {
            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.DeletedAt == null)
                .ToListAsync();
            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
            return View(new TeacherCourse());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacherCourse(TeacherCourse teacherCourse)
        {
            if (ModelState.IsValid)
            {
                // Check if already exists
                var exists = await _context.TeacherCourses
                    .AnyAsync(tc => tc.TeacherId == teacherCourse.TeacherId
                                 && tc.CourseId == teacherCourse.CourseId
                                 && tc.DeletedAt == null);

                if (exists)
                {
                    TempData["ErrorMessage"] = "هذا الربط موجود بالفعل";
                    ViewBag.Teachers = await _context.Teachers
                        .Where(t => t.DeletedAt == null)
                        .ToListAsync();
                    ViewBag.Courses = await _context.Courses
                        .Where(c => c.DeletedAt == null)
                        .ToListAsync();
                    return View(teacherCourse);
                }

                teacherCourse.CreatedAt = DateTime.Now;
                _context.TeacherCourses.Add(teacherCourse);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "تم ربط الأستاذ بالكورس بنجاح";
                return RedirectToAction(nameof(TeacherCourses));
            }

            ViewBag.Teachers = await _context.Teachers
                .Where(t => t.DeletedAt == null)
                .ToListAsync();
            ViewBag.Courses = await _context.Courses
                .Where(c => c.DeletedAt == null)
                .ToListAsync();
            return View(teacherCourse);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacherCourse(int id)
        {
            var teacherCourse = await _context.TeacherCourses.FindAsync(id);
            if (teacherCourse == null)
                return NotFound();

            teacherCourse.DeletedAt = DateTime.Now;
            teacherCourse.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الربط بنجاح";
            return RedirectToAction(nameof(TeacherCourses));
        }

        // ==================== HELPER METHODS ====================
        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.Id == id && e.DeletedAt == null);
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id && e.DeletedAt == null);
        }

        private bool VideoExists(int id)
        {
            return _context.CourseVideos.Any(e => e.Id == id && e.DeletedAt == null);
        }
        private List<SelectListItem> GetGradesList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem("الصف الأول", "1"),
                new SelectListItem("الصف الثاني", "2"),
                new SelectListItem("الصف الثالث", "3"),
                new SelectListItem("الصف الرابع", "4"),
                new SelectListItem("الصف الخامس", "5"),
                new SelectListItem("الصف السادس", "6"),
                new SelectListItem("الصف السابع", "7"),
                new SelectListItem("الصف الثامن", "8"),
                new SelectListItem("الصف التاسع", "9"),
                new SelectListItem("الصف العاشر", "10"),
                new SelectListItem("الصف الحادي عشر", "11"),
                new SelectListItem("الصف الثاني عشر", "12")
            };
        }

        private string GetGradeName(int grade)
        {
            return grade switch
            {
                1 => "الصف الأول",
                2 => "الصف الثاني",
                3 => "الصف الثالث",
                4 => "الصف الرابع",
                5 => "الصف الخامس",
                6 => "الصف السادس",
                7 => "الصف السابع",
                8 => "الصف الثامن",
                9 => "الصف التاسع",
                10 => "الصف العاشر",
                11 => "الصف الحادي عشر",
                12 => "الصف الثاني عشر",
                _ => $"الصف {grade}"
            };
        }

        private async Task<bool> TeacherGradeAssignmentExists(int id)
        {
            return await _context.Set<TeacherGradeAssignment>()
                .AnyAsync(a => a.Id == id && a.DeletedAt == null);
        }
        // ==================== API ENDPOINTS ====================
        [HttpGet]
        public async Task<IActionResult> GetTeacherDetails(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherCourses)
                .ThenInclude(tc => tc.Course)
                .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);

            if (teacher == null)
                return Json(new { success = false, message = "الأستاذ غير موجود" });

            return Json(new
            {
                success = true,
                teacher = new
                {
                    teacher.Id,
                    teacher.Name,
                    teacher.Subject,
                    teacher.Bio,
                    teacher.Image,
                    courses = teacher.TeacherCourses
                        .Where(tc => tc.DeletedAt == null)
                        .Select(tc => new
                        {
                            tc.Course.Id,
                            tc.Course.Name,
                            tc.Course.Price
                        })
                }
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetCourseDetails(int id)
        {
            var course = await _context.Courses
                .Include(c => c.TeacherCourses)
                .ThenInclude(tc => tc.Teacher)
                .Include(c => c.CourseVideos)
                .FirstOrDefaultAsync(c => c.Id == id && c.DeletedAt == null);

            if (course == null)
                return Json(new { success = false, message = "الكورس غير موجود" });

            return Json(new
            {
                success = true,
                course = new
                {
                    course.Id,
                    course.Name,
                    course.Description,
                    course.Price,
                    teachers = course.TeacherCourses
                        .Where(tc => tc.DeletedAt == null)
                        .Select(tc => new
                        {
                            tc.Teacher.Id,
                            tc.Teacher.Name
                        }),
                    videos = course.CourseVideos
                        .Where(v => v.DeletedAt == null)
                        .Select(v => new
                        {
                            v.Id,
                            v.Title,
                            v.Order,
                            v.IsFree
                        })
                }
            });
        }
    }
}

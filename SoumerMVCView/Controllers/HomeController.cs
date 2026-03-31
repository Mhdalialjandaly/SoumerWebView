// SoumerMVCView/Controllers/HomeController.cs
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;
using SoumerMVCView.Models;
using SoumerMVCView.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SoumerMVCView.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherAssignmentService _assignmentService;

        public HomeController(
            ILogger<HomeController> logger,
            ITeacherRepository teacherRepository,
            ITeacherAssignmentService assignmentService)
        {
            _logger = logger;
            _teacherRepository = teacherRepository;
            _assignmentService = assignmentService;
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
            ViewBag.Teachers = allTeachers;
            return View();
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
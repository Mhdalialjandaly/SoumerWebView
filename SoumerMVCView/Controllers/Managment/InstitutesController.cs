using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoumerMVCView.Models;
using SoumerMVCView.Services.TeacherAssignmentService;

namespace SoumerMVCView.Controllers.Managment
{
    [Authorize]
    public class InstitutesController : Controller
    {
        private readonly ILogger<InstitutesController> _logger;
        private readonly ITeacherRepository _teacherRepository;
        private readonly ITeacherAssignmentService _assignmentService;

        public InstitutesController(
            ILogger<InstitutesController> logger,
            ITeacherRepository teacherRepository,
            ITeacherAssignmentService assignmentService)
        {
            _logger = logger;
            _teacherRepository = teacherRepository;
            _assignmentService = assignmentService;
        }

        // عرض صفحة المعاهد
        public async Task<IActionResult> Index()
        {
            var model = new InstitutesViewModel
            {
                Teachers = await _teacherRepository.GetAll(),
                GradesTeachers = await _assignmentService.GetOrganizedGradesTeachers()
            };

            return View(model);
        }

        // الحصول على بيانات المعاهد (API)
        [HttpGet]
        public async Task<IActionResult> GetInstitutesData()
        {
            try
            {
                var institutes = await _teacherRepository.GetAll();
                return Json(new { success = true, institutes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting institutes data");
                return Json(new { success = false, message = "حدث خطأ في تحميل بيانات المعاهد" });
            }
        }

        // الحصول على تفاصيل معهد معين
        [HttpGet]
        public async Task<IActionResult> GetInstituteDetails(int id)
        {
            try
            {
                var teacher = await _teacherRepository.GetById(id);
                if (teacher == null)
                {
                    return Json(new { success = false, message = "المعهد غير موجود" });
                }

                return Json(new { success = true, institute = teacher });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting institute details for ID: {Id}", id);
                return Json(new { success = false, message = "حدث خطأ في تحميل بيانات المعهد" });
            }
        }
    }
}

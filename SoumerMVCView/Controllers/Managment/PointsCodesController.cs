using DataAccess;
using DataAccess.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Controllers.Managment
{
    [Authorize(Roles = "Admin")]
    public class PointsCodesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBalanceService _balanceService;
        private readonly ILogger<PointsCodesController> _logger;

        public PointsCodesController(
            ApplicationDbContext context,
            IBalanceService balanceService,
            ILogger<PointsCodesController> logger)
        {
            _context = context;
            _balanceService = balanceService;
            _logger = logger;
        }

        // عرض قائمة الأكواد
        public async Task<IActionResult> Index()
        {
            var validCodes = await _balanceService.GetValidCodes();
            var usedCodes = await _balanceService.GetUsedCodes();

            var model = new PointsCodesViewModel
            {
                ValidCodes = validCodes,
                UsedCodes = usedCodes
            };

            return View(model);
        }

        // عرض صفحة إنشاء أكواد جديدة
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreatePointsCodeDto());
        }

        // إنشاء أكواد جديدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePointsCodeDto model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var codes = await _balanceService.GenerateCodes(
                        model.PointsValue,
                        model.NumberOfCodes,
                        model.ExpiryDate
                    );

                    if (codes.Any())
                    {
                        TempData["SuccessMessage"] = $"تم توليد {codes.Count} كود بنجاح";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "حدث خطأ أثناء توليد الأكواد");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return View(model);
        }

        // حذف كود
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var code = await _context.Set<PointsCode>().FindAsync(id);
            if (code == null)
                return NotFound();

            code.DeletedAt = DateTime.Now;
            code.DeletedBy = User.Identity?.Name;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "تم حذف الكود بنجاح";
            return RedirectToAction(nameof(Index));
        }

        // عرض تفاصيل كود
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var code = await _context.Set<PointsCode>()
                .Include(pc => pc.UsedByUser)
                .FirstOrDefaultAsync(pc => pc.Id == id && pc.DeletedAt == null);

            if (code == null)
                return NotFound();

            return View(code);
        }

        // تصدير الأكواد
        [HttpGet]
        public async Task<IActionResult> ExportCodes()
        {
            var codes = await _balanceService.GetValidCodes();

            var content = string.Join("\n", codes.Select(c => c.Code));
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);

            return File(bytes, "text/plain", $"points-codes-{DateTime.Now:yyyy-MM-dd}.txt");
        }
    }
}
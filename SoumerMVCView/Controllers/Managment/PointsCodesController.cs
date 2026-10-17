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

        [HttpGet]
        public async Task<IActionResult> Report(
            string status = "all",
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string searchCode = null,
            bool export = false)
        {
            try
            {
                // جلب جميع الأكواد (مع المستخدم)
                var query = _context.Set<PointsCode>()
                    .Include(pc => pc.UsedByUser)
                    .AsNoTracking()
                    .AsQueryable();

                // الفلترة بالتاريخ
                if (fromDate.HasValue)
                    query = query.Where(pc => pc.CreatedAt >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pc => pc.CreatedAt < toDate.Value.AddDays(1));

                // فلترة البحث بالكود
                if (!string.IsNullOrWhiteSpace(searchCode))
                    query = query.Where(pc => pc.Code.Contains(searchCode.Trim()));

                var allCodes = await query.OrderByDescending(pc => pc.CreatedAt).ToListAsync();

                // تطبيق فلترة الحالة (بعد جلب البيانات لأن IsExpired/IsValid محسوبة)
                IEnumerable<PointsCode> filtered = allCodes;
                if (status == "valid")
                    filtered = allCodes.Where(c => !c.IsUsed && !c.DeletedAt.HasValue && !(c.ExpiryDate.HasValue && c.ExpiryDate < DateTime.Now));
                else if (status == "used")
                    filtered = allCodes.Where(c => c.IsUsed);
                else if (status == "expired")
                    filtered = allCodes.Where(c => c.ExpiryDate.HasValue && c.ExpiryDate < DateTime.Now && !c.IsUsed);

                var codeList = filtered.ToList();

                // ====== الإحصائيات ======
                var validList = allCodes.Where(c => !c.IsUsed && !c.DeletedAt.HasValue
                                            && !(c.ExpiryDate.HasValue && c.ExpiryDate < DateTime.Now)).ToList();
                var usedList = allCodes.Where(c => c.IsUsed).ToList();
                var expiredList = allCodes.Where(c => c.ExpiryDate.HasValue
                                            && c.ExpiryDate < DateTime.Now
                                            && !c.IsUsed).ToList();

                var model = new PointsCodesReportViewModel
                {
                    Status = status,
                    FromDate = fromDate,
                    ToDate = toDate,
                    SearchCode = searchCode,

                    TotalCodes = allCodes.Count,
                    ValidCodes = validList.Count,
                    UsedCodes = usedList.Count,
                    ExpiredCodes = expiredList.Count,
                    DeletedCodes = allCodes.Count(c => c.DeletedAt.HasValue),

                    TotalPointsValue = allCodes.Sum(c => c.PointsValue),
                    ValidPointsValue = validList.Sum(c => c.PointsValue),
                    UsedPointsValue = usedList.Sum(c => c.PointsValue),
                    ExpiredPointsValue = expiredList.Sum(c => c.PointsValue),

                    Codes = codeList.Select(c => new PointsCodeDto
                    {
                        Id = c.Id,
                        Code = c.Code,
                        PointsValue = c.PointsValue,
                        IsUsed = c.IsUsed,
                        UsedByUserId = c.UsedByUserId,
                        UsedByUserName = c.UsedByUser?.UserName ?? c.UsedByUser?.Email,
                        UsedAt = c.UsedAt,
                        ExpiryDate = c.ExpiryDate,
                        CreatedBy = c.CreatedBy,
                        CreatedAt = c.CreatedAt
                    }).ToList()
                };

                // ====== آخر 30 يوماً ======
                var from30 = DateTime.Now.AddDays(-30).Date;
                model.DailyUsage = usedList
                    .Where(c => c.UsedAt.HasValue && c.UsedAt.Value >= from30)
                    .GroupBy(c => c.UsedAt.Value.Date)
                    .Select(g => new DailyUsageDto
                    {
                        Date = g.Key,
                        Count = g.Count(),
                        Points = g.Sum(c => c.PointsValue)
                    })
                    .OrderBy(x => x.Date)
                    .ToList();

                // ====== أعلى 5 مستخدمين ======
                model.TopUsers = usedList
                    .Where(c => !string.IsNullOrEmpty(c.UsedByUserId))
                    .GroupBy(c => new { c.UsedByUserId, c.UsedByUser?.UserName })
                    .Select(g => new TopUserDto
                    {
                        UserId = g.Key.UsedByUserId,
                        UserName = g.Key.UserName ?? "غير معروف",
                        CodesCount = g.Count(),
                        TotalPoints = g.Sum(c => c.PointsValue)
                    })
                    .OrderByDescending(x => x.TotalPoints)
                    .Take(5)
                    .ToList();

                if (export)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("الكود,القيمة,الحالة,المستخدم,تاريخ الإنشاء,تاريخ الانتهاء,تاريخ الاستخدام,أنشأه");

                    foreach (var code in model.Codes)
                    {
                        var statusText = code.IsUsed ? "مستخدم"
                                       : code.IsExpired ? "منتهي"
                                       : "صالح";

                        sb.AppendLine(string.Join(",",
                            $"\"{code.Code}\"",
                            code.PointsValue.ToString("0.##"),
                            statusText,
                            $"\"{code.UsedByUserName ?? ""}\"",
                            code.CreatedAt.ToString("yyyy/MM/dd HH:mm"),
                            code.ExpiryDate?.ToString("yyyy/MM/dd") ?? "لا ينتهي",
                            code.UsedAt?.ToString("yyyy/MM/dd HH:mm") ?? "",
                            $"\"{code.CreatedBy ?? ""}\""
                        ));
                    }

                    var bytes = System.Text.Encoding.UTF8.GetPreamble()
                        .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString()))
                        .ToArray();

                    return File(bytes, "text/csv", $"points-codes-report-{DateTime.Now:yyyy-MM-dd-HHmm}.csv");
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating points codes report");
                TempData["ErrorMessage"] = "حدث خطأ أثناء إنشاء التقرير";
                return RedirectToAction(nameof(Index));
            }
        }


    }
}
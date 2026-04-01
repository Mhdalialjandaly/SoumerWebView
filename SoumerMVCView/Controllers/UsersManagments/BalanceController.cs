using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SoumerMVCView.Models;
using SoumerMVCView.Services.BalanceService;
using System.Security.Claims;

namespace SoumerMVCView.Controllers.UsersManagments
{
    [Authorize]
    public class BalanceController : Controller
    {
        private readonly IBalanceService _balanceService;
        private readonly ILogger<BalanceController> _logger;

        public BalanceController(IBalanceService balanceService, ILogger<BalanceController> logger)
        {
            _balanceService = balanceService;
            _logger = logger;
        }

        // عرض صفحة النقاط الرئيسية
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var model = await _balanceService.GetUserBalance(userId);
            return View(model);
        }

        // عرض تاريخ المعاملات
        public async Task<IActionResult> Transactions(int page = 1, int pageSize = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var transactions = await _balanceService.GetUserTransactions(userId, page, pageSize);
            var balance = await _balanceService.GetUserBalance(userId);

            var model = new BalanceViewModel
            {
                CurrentBalance = balance.CurrentBalance,
                RecentTransactions = transactions,
                TotalPoints = balance.TotalPoints,
                CurrentPage = page,
                TotalPages = (transactions.Count + pageSize - 1) / pageSize
            };

            return View(model);
        }

        // إضافة نقاط (للأدمن فقط)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddPoints(string userId, decimal amount, string description)
        {
            try
            {
                if (amount <= 0)
                    return Json(new { success = false, message = "المبلغ يجب أن يكون أكبر من صفر" });

                var transaction = await _balanceService.AddPoints(userId, amount, description);
                if (transaction != null)
                    return Json(new { success = true, message = "تم إضافة النقاط بنجاح" });

                return Json(new { success = false, message = "حدث خطأ أثناء إضافة النقاط" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding points to user {UserId}", userId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // خصم نقاط (للأدمن فقط)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> DeductPoints(string userId, decimal amount, string description)
        {
            try
            {
                if (amount <= 0)
                    return Json(new { success = false, message = "المبلغ يجب أن يكون أكبر من صفر" });

                var transaction = await _balanceService.DeductPoints(userId, amount, description);
                if (transaction != null)
                    return Json(new { success = true, message = "تم خصم النقاط بنجاح" });

                return Json(new { success = false, message = "الرصيد غير كافي أو حدث خطأ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deducting points from user {UserId}", userId);
                return Json(new { success = false, message = ex.Message });
            }
        }

        // تحويل نقاط لمستخدم آخر
        [HttpPost]
        public async Task<IActionResult> TransferPoints(string toUserId, decimal amount, string description)
        {
            try
            {
                var fromUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(fromUserId))
                    return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً" });

                if (fromUserId == toUserId)
                    return Json(new { success = false, message = "لا يمكن تحويل النقاط لنفس المستخدم" });

                if (amount <= 0)
                    return Json(new { success = false, message = "المبلغ يجب أن يكون أكبر من صفر" });

                var result = await _balanceService.TransferPoints(fromUserId, toUserId, amount, description);
                if (result)
                    return Json(new { success = true, message = "تم تحويل النقاط بنجاح" });

                return Json(new { success = false, message = "الرصيد غير كافي أو حدث خطأ" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring points");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // الحصول على الرصيد الحالي (API)
        [HttpGet]
        public async Task<IActionResult> GetCurrentBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, balance = 0 });

            var balance = await _balanceService.GetUserBalance(userId);
            return Json(new { success = true, balance = balance.CurrentBalance });
        }
    }
}
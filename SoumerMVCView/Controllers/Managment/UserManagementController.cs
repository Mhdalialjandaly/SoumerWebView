using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Controllers.Managment
{
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserManagementController> _logger;
        private readonly UserManager<User> _userManager;
        public UserManagementController(
            IUserRepository userRepository,
            RoleManager<IdentityRole> roleManager,
            UserManager<User> userManager,
            ILogger<UserManagementController> logger)
        {
            _userRepository = userRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        // عرض قائمة المستخدمين
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAll();
            return View(users);
        }

        // صفحة إنشاء مستخدم جديد
        [HttpGet]
        public IActionResult Create()
        {
            return View(new CreateUserDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                // التحقق من عدم وجود اسم مستخدم أو بريد إلكتروني مكرر
                if (await _userRepository.IsUserNameExist(model.UserName))
                    ModelState.AddModelError("UserName", "اسم المستخدم موجود مسبقاً");
                if (await _userRepository.IsEmailExist(model.Email))
                    ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل");

                if (!ModelState.IsValid)
                    return View(model);

                await _userRepository.Create(model);
                TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                ModelState.AddModelError("", "حدث خطأ: " + ex.Message);
                return View(model);
            }
        }

        // صفحة تعديل المستخدم
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound();

            var model = new UpdateUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Description = user.Description,
                IsActive = user.IsActive
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, UpdateUserDto model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (await _userRepository.IsUserNameExist(model.UserName, id))
                    ModelState.AddModelError("UserName", "اسم المستخدم موجود مسبقاً");
                if (await _userRepository.IsEmailExist(model.Email, id))
                    ModelState.AddModelError("Email", "البريد الإلكتروني مستخدم بالفعل");

                if (!ModelState.IsValid)
                    return View(model);

                await _userRepository.Update(model);
                TempData["SuccessMessage"] = "تم تحديث بيانات المستخدم بنجاح";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                ModelState.AddModelError("", "حدث خطأ: " + ex.Message);
                return View(model);
            }
        }

        // حذف نهائي (غير مستحسن، نفضل soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _userRepository.Delete(id);
                TempData["SuccessMessage"] = "تم حذف المستخدم نهائياً";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                TempData["ErrorMessage"] = "فشل الحذف: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // تعطيل المستخدم (soft delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SoftDelete(string id)
        {
            try
            {
                await _userRepository.SoftDelete(id);
                TempData["SuccessMessage"] = "تم تعطيل المستخدم بنجاح";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting user");
                TempData["ErrorMessage"] = "فشل التعطيل: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // إعادة تفعيل المستخدم
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            try
            {
                await _userRepository.Restore(id);
                TempData["SuccessMessage"] = "تم إعادة تفعيل المستخدم بنجاح";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring user");
                TempData["ErrorMessage"] = "فشل الاستعادة: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // عرض تفاصيل المستخدم
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound();

            var roles = await _userRepository.GetUserRoles(id);
            ViewBag.Roles = roles;
            return View(user);
        }

        // صفحة تغيير كلمة المرور (للأدمن)
        [HttpGet]
        public IActionResult ChangePassword(string id)
        {
            ViewBag.UserId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string id, string newPassword, string confirmPassword)
        {
            // ... التحقق
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح";
                return RedirectToAction(nameof(Index));
            }
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);
            return View();
        }

        // إدارة أدوار المستخدم
        [HttpGet]
        public async Task<IActionResult> ManageRoles(string id)
        {
            var user = await _userRepository.GetById(id);
            if (user == null)
                return NotFound();

            var userRoles = await _userRepository.GetUserRoles(id);
            var allRoles = _roleManager.Roles.Select(r => r.Name).ToList();

            var model = new UserRolesViewModel
            {
                UserId = id,
                UserName = user.UserName,
                UserRoles = userRoles,
                AllRoles = allRoles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageRoles(UserRolesViewModel model)
        {
            try
            {
                var userRoles = await _userRepository.GetUserRoles(model.UserId);
                // إزالة أدوار قديمة وإضافة الجديدة (يمكنك تحسينها)
                // لكن سنستخدم طريقة بسيطة: إزالة الكل ثم إضافة المختارة
                var user = await _userRepository.GetById(model.UserId);
                if (user == null) return NotFound();

                // استخدم UserManager مباشر لتعديل الأدوار
                var userEntity = await _userManager.FindByIdAsync(model.UserId);
                var currentRoles = await _userManager.GetRolesAsync(userEntity);
                await _userManager.RemoveFromRolesAsync(userEntity, currentRoles);
                if (model.SelectedRoles != null && model.SelectedRoles.Any())
                    await _userManager.AddToRolesAsync(userEntity, model.SelectedRoles);

                TempData["SuccessMessage"] = "تم تحديث الأدوار بنجاح";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error managing roles");
                TempData["ErrorMessage"] = "فشل تحديث الأدوار: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }


    }
}

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
            try
            {
                if (await _userRepository.IsEmailExist(model.Email))
                {
                    ModelState.AddModelError("Email", "البريد الإلكتروني موجود بالفعل");
                    return View(model);
                }

                // معالجة رفع الصورة
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    var fileName = await SaveAvatarImage(model.AvatarFile);
                    model.AvatarUrl = fileName;
                }

                var user = await _userRepository.Create(model);

                if (user != null)
                {
                    TempData["SuccessMessage"] = "تم إنشاء المستخدم بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "حدث خطأ أثناء إنشاء المستخدم");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
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
        public async Task<IActionResult> Edit(UpdateUserDto model)
        {
            try
            {
                // معالجة رفع الصورة
                if (model.AvatarFile != null && model.AvatarFile.Length > 0)
                {
                    var fileName = await SaveAvatarImage(model.AvatarFile);
                    model.AvatarUrl = fileName;
                }
                else if (string.IsNullOrEmpty(model.AvatarUrl))
                {
                    // إذا تم إزالة الصورة
                    model.AvatarUrl = null;
                }

                var result = await _userRepository.Update(model);

                if (result)
                {
                    TempData["SuccessMessage"] = "تم تحديث بيانات المستخدم بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "حدث خطأ أثناء تحديث المستخدم");
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        private async Task<string> SaveAvatarImage(IFormFile file)
        {
            try
            {
                // التحقق من نوع الملف
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    throw new Exception("نوع الملف غير مدعوم. يرجى رفع صورة بصيغة JPG, PNG, أو GIF");
                }

                // التحقق من حجم الملف (الحد الأقصى 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    throw new Exception("حجم الصورة كبير جداً. الحد الأقصى 5MB");
                }

                // إنشاء اسم فريد للملف
                var fileName = $"{Guid.NewGuid()}{extension}";
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

                // إنشاء المجلد إذا لم يكن موجوداً
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, fileName);

                // حفظ الملف
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return $"/uploads/avatars/{fileName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"حدث خطأ أثناء حفظ الصورة: {ex.Message}");
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

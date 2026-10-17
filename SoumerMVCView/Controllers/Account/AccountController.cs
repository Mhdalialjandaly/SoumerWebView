using DataAccess.Entities;
using DataAccess.IRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models;
using SoumerMVCView.Models;

namespace SoumerMVCView.Controllers.Account
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            ILogger<AccountController> logger,
            IUserRepository userRepository)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _userRepository = userRepository;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // إذا كان المستخدم مسجل دخول بالفعل، أعده إلى الصفحة الرئيسية
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // البحث عن المستخدم بالبريد الإلكتروني
                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                    return View(model);
                }

                // التحقق من أن الحساب غير محذوف
                if (user.DeletedAt != null)
                {
                    ModelState.AddModelError(string.Empty, "هذا الحساب غير نشط. يرجى التواصل مع الدعم الفني");
                    return View(model);
                }

                // التحقق من أن الحساب نشط
                if (!user.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "هذا الحساب معطل. يرجى التواصل مع الدعم الفني");
                    return View(model);
                }

                // محاولة تسجيل الدخول
                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {Email} logged in successfully", model.Email);

                    // تحديث آخر وقت تسجيل دخول
                    user.LastLogin = DateTime.Now;
                    await _userManager.UpdateAsync(user);

                    // التوجيه إلى الصفحة المطلوبة أو الرئيسية
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "الحساب مقفل. يرجى المحاولة لاحقاً");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "البريد الإلكتروني أو كلمة المرور غير صحيحة");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء محاولة تسجيل الدخول. يرجى المحاولة لاحقاً");
                return View(model);
            }
        }

        // ==========================================
        // GET: /Account/Register
        // ==========================================
        [HttpGet]
        public IActionResult Register(string returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new RegisterViewModel());
        }

        // ==========================================
        // POST: /Account/Register
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string returnUrl = null)
        {
            _logger.LogInformation("Register POST received for {Email}, ModelState valid: {IsValid}",
               model?.Email, ModelState.IsValid);

            if (!model.AcceptTerms)
            {
                ModelState.AddModelError(nameof(model.AcceptTerms), "يجب الموافقة على الشروط والأحكام");
            }

            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // التحقق من عدم وجود البريد الإلكتروني مسبقاً
                if (await _userRepository.IsEmailExist(model.Email))
                {
                    ModelState.AddModelError(nameof(model.Email), "البريد الإلكتروني مستخدم بالفعل");
                    return View(model);
                }

                // التحقق من عدم وجود اسم المستخدم مسبقاً
                if (await _userRepository.IsUserNameExist(model.UserName))
                {
                    ModelState.AddModelError(nameof(model.UserName), "اسم المستخدم مستخدم بالفعل");
                    return View(model);
                }

                // إنشاء المستخدم باستخدام الـ Repository
                var createUserDto = new CreateUserDto
                {
                    UserName = model.UserName.Trim(),
                    FullName = model.FullName.Trim(),
                    Email = model.Email.Trim(),
                    Password = model.Password,
                    Description = model.Description,
                    PhoneNumber = model.PhoneNumber,
                    IsActive = true,
                    AvatarUrl = null
                };

                var createdUser = await _userRepository.Create(createUserDto);

                if (createdUser == null)
                {
                    ModelState.AddModelError(string.Empty, "حدث خطأ أثناء إنشاء الحساب. يرجى المحاولة لاحقاً");
                    return View(model);
                }

                // إسناد الدور الافتراضي (Student)
                try
                {
                    await _userRepository.AssignToRole(createdUser.Id, "Student");
                }
                catch (Exception roleEx)
                {
                    _logger.LogWarning(roleEx, "Failed to assign default role for user {Email}", model.Email);
                    // لا نوقف العملية إذا فشل إسناد الدور
                }

                _logger.LogInformation("New user registered: {Email}", model.Email);

                // تسجيل الدخول تلقائياً بعد التسجيل
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "حدث خطأ أثناء محاولة إنشاء الحساب. يرجى المحاولة لاحقاً");
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Settings()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (!string.IsNullOrEmpty(model.Username))
                user.UserName = model.Username;

            if (!string.IsNullOrEmpty(model.Email))
                user.Email = model.Email;

            if (!string.IsNullOrEmpty(model.PhoneNumber))
                user.PhoneNumber = model.PhoneNumber;

            if (!string.IsNullOrEmpty(model.Description))
                user.Description = model.Description;

            user.ModifiedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok();

            return BadRequest(result.Errors);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
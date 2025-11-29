using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetworkCafesControllers.Models.Entities;
using NetworkCafesControllers.Models.ViewModels.Auth;
using NetworkCafesControllers.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace NetworkCafesControllers.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAuthService _authService;
        private readonly IEmailSender _emailSender;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAuthService authService,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _authService = authService;
            _emailSender = emailSender;
        }

        #region Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUserByName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByName != null)
            {
                ModelState.AddModelError("UserName", "اسم المستخدم مستخدم بالفعل.");
                return View(model);
            }

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "البريد الإلكتروني مسجل بالفعل.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            if (!(await _userManager.GetUsersInRoleAsync("Admin")).Any())
                await _userManager.AddToRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "user");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var callbackUrl = Url.Action(
                nameof(ConfirmEmail),
                "Account",
                new { userId = user.Id, token },
                protocol: Request.Scheme);

            var body = $@"
                <h2>مرحبًا {user.UserName}</h2>
                <p>شكرًا لتسجيلك  .</p>
                <p>من فضلك اضغط على الزر التالي لتأكيد بريدك الإلكتروني:</p>
                <p>
                    <a href=""{callbackUrl}"" 
                       style=""display:inline-block;padding:10px 20px;background-color:#198754;color:#fff;text-decoration:none;border-radius:4px;"">
                        تأكيد البريد الإلكتروني
                    </a>
                </p>
                <p>إذا لم تقم بالتسجيل، يمكنك تجاهل هذه الرسالة.</p>";

            await _emailSender.SendEmailAsync(user.Email, "تأكيد البريد الإلكتروني", body);

            return View("RegisterConfirmation");
        }
        #endregion

        #region ConfirmEmail
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return View("ConfirmEmailFailed");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return View("ConfirmEmailFailed");

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
                return View("ConfirmEmailSuccess");

            return View("ConfirmEmailFailed");
        }
        #endregion

        #region Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "brands");

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // نحاول نجيب اليوزر باليوزرنيم أو الإيميل
            ApplicationUser? user = null;
            if (!string.IsNullOrWhiteSpace(model.UserNameOrEmail))
            {
                var input = model.UserNameOrEmail.Trim();

                user = await _userManager.Users
                    .FirstOrDefaultAsync(u =>
                        u.UserName == input ||
                        u.Email == input);
            }

            // لو مش لاقي يوزر أصلاً -> ما نكشفش إذا كان موجود ولا لأ
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "اسم المستخدم أو كلمة المرور غير صحيحة.");
                return View(model);
            }

            // محاولة تسجيل الدخول مع تفعيل الـ lockoutOnFailure
            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                //var user = await _userManager.FindByNameAsync(model.UserName);
                var theme = user?.ThemePreference ?? "system";

                Response.Cookies.Append(
                    "theme",
                    theme,
                    new CookieOptions
                    {
                        HttpOnly = false,
                        IsEssential = true,
                        Expires = DateTimeOffset.UtcNow.AddYears(1)
                    });

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Brands");
            }

            // لو الإيميل مش متأكد أو ممنوع يدخل
            if (result.IsNotAllowed)
            {
                ModelState.AddModelError(string.Empty, "يجب تأكيد البريد الإلكتروني أولًا قبل تسجيل الدخول.");
            }
            // لو الأكاونت اتقفل مؤقتًا بسبب محاولات فاشلة
            else if (result.IsLockedOut)
            {
                var lockoutSpan = _userManager.Options.Lockout.DefaultLockoutTimeSpan;
                var minutes = (int)Math.Round(lockoutSpan.TotalMinutes);

                ModelState.AddModelError(string.Empty,
                    $"تم قفل الحساب مؤقتًا بسبب محاولات تسجيل دخول خاطئة متكررة. " +
                    $"برجاء المحاولة مرة أخرى بعد {(minutes > 0 ? minutes + " دقيقة" : lockoutSpan.TotalSeconds + " ثانية")}.");
            }
            else
            {
                var failedCount = await _userManager.GetAccessFailedCountAsync(user);
                var maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                var remaining = maxAttempts - failedCount;
                if (remaining < 0) remaining = 0;
                var msg =
                    $"اسم المستخدم أو كلمة المرور غير صحيحة. " +
                    $"عدد المحاولات الخاطئة الحالية: {failedCount} من {maxAttempts}.";

                if (remaining > 0)
                {
                    msg += $" سيتم قفل الحساب مؤقتًا بعد {remaining} محاولة فاشلة إضافية.";
                }

                ModelState.AddModelError(string.Empty, msg);
            }

            return View(model);
        }
        #endregion

        #region Logout
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return RedirectToAction(nameof(Login));
        }
        #endregion

        #region ChangePassword
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View(new ChangePasswordViewModel());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId is null)
                return RedirectToAction(nameof(Login));

            var result = await _authService.ChangePasswordAsync(userId, model);

            if (result.Succeeded)
            {
                TempData["Success"] = "تم تعديل كلمة المرور بنجاح.";
                return RedirectToAction("Index", "Brands");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
        #endregion

        #region Forgot / Reset Password
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var normalizedEmail = model.Email.Trim().ToUpper();

            var user = await _userManager.Users
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);

            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                return View("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var callbackUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { userId = user.Id, token },
                protocol: Request.Scheme);

            var body = $@"
            <h2> لإعادة تعيين كلمة المرور</h2>
            <p>لقد طلبت إعادة تعيين كلمة المرور لحسابك.</p>
            <p> المستخدم المرتبط بهذا الحساب : {user}</p>
            <p> هذا الرابط صالح لمدة 24 ساعة فقط</p>

            <p>اضغط على الزر التالي لاختيار كلمة مرور جديدة:</p>
            <p>
                <a href=""{callbackUrl}""
                   style=""display:inline-block;padding:10px 20px;background-color:#0d6efd;color:#fff;text-decoration:none;border-radius:4px;"">
                    إعادة تعيين كلمة المرور
                </a>
            </p>";

            await _emailSender.SendEmailAsync(user.Email, "إعادة تعيين كلمة المرور", body);

            return View("ForgotPasswordConfirmation");
        }

        [AllowAnonymous]
        public IActionResult ResetPassword(string userId, string token)
        {
            if (userId == null || token == null)
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return RedirectToAction(nameof(Login));

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (result.Succeeded)
                return View("ResetPasswordSuccess");

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }
        #endregion

        #region ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ExternalLogin(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return Challenge(properties, provider);
        }
        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                TempData["Error"] = $"حدث خطأ أثناء تسجيل الدخول الخارجي: {remoteError}";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                TempData["Error"] = "تعذر تحميل بيانات تسجيل الدخول من الموفر الخارجي.";
                return RedirectToAction(nameof(Login), new { returnUrl });
            }

            var signInResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            if (signInResult.IsLockedOut)
            {
                return View("Lockout");
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            var name = info.Principal.FindFirstValue(ClaimTypes.Name);

            if (email == null)
            {
                TempData["Error"] = "الموفر الخارجي لم يرسل بريدًا إلكترونيًا صالحًا.";
                return RedirectToAction(nameof(Login));
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View("Login");
                }
            }

            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                foreach (var error in addLoginResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View("Login");
            }

            await _signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect(returnUrl);
        }

        #endregion




        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}

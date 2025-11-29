using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetworkCafesControllers.Models.Entities;
using NetworkCafesControllers.Models.Identity;
using NetworkCafesControllers.Models.ViewModels.Auth;
using NetworkCafesControllers.Services.Interfaces;

namespace NetworkCafesControllers.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        public async Task<IdentityResult> RegisterAsync(RegisterViewModel model)
        {
            // إنشاء المستخدم
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return result;

            // تأكد إن الـ Roles موجودة
            if (!await _roleManager.Roles.AnyAsync())
            {
                await _roleManager.CreateAsync(new IdentityRole(AppRoles.Admin));
                await _roleManager.CreateAsync(new IdentityRole(AppRoles.CafeUser));
            }

            // أول يوزر نخليه Admin، الباقي CafeUser مثلاً
            var isFirstUser = (await _userManager.Users.CountAsync()) == 1;
            var role = isFirstUser ? AppRoles.Admin : AppRoles.CafeUser;

            await _userManager.AddToRoleAsync(user, role);

            return result;
        }

        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            ApplicationUser? user;

            if (model.UserNameOrEmail.Contains("@"))
                user = await _userManager.FindByEmailAsync(model.UserNameOrEmail);
            else
                user = await _userManager.FindByNameAsync(model.UserNameOrEmail);

            if (user == null)
                return SignInResult.Failed;

            return await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "المستخدم غير موجود." });

            return await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
        }
    }
}

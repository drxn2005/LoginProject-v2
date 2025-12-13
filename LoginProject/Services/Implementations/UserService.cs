using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NetworkCafesControllers.Models.Entities;
using NetworkCafesControllers.Models.ViewModels.Admin;
using NetworkCafesControllers.Services.Interfaces;

namespace NetworkCafesControllers.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // تحديث الدالة GetUsersAsync في السطر 40:
        public async Task<(List<UserViewModel> Users, int TotalCount)> GetUsersAsync(int page = 1, int pageSize = 10, string search = "")
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(u =>
                    u.UserName.Contains(search) ||
                    u.Email.Contains(search) ||
                    u.PhoneNumber.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    UserName = u.UserName!,
                    Email = u.Email!,
                    PhoneNumber = u.PhoneNumber,
                    EmailConfirmed = u.EmailConfirmed,
                    IsLockedOut = u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow,
                    LockoutEnd = u.LockoutEnd,
                    AccessFailedCount = u.AccessFailedCount,
                    RegistrationDate = u.CreatedAt,
                    LastLoginDate = u.LastLoginDate
                })
                .ToListAsync();

            foreach (var user in users)
            {
                user.Roles = await GetUserRolesAsync(user.Id);
            }

            return (users, totalCount);
        }

        // تحديث الدالة GetUserByIdAsync في السطر 88:
        public async Task<UserViewModel?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await GetUserRolesAsync(userId);

            return new UserViewModel
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles,
                RegistrationDate = user.CreatedAt,
                LastLoginDate = user.LastLoginDate
            };
        }
       public async Task<IdentityResult> CreateUserAsync(CreateUserViewModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                EmailConfirmed = model.EmailConfirmed
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded && model.SelectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);
            }

            return result;
        }

        public async Task<IdentityResult> UpdateUserAsync(EditUserViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "المستخدم غير موجود" });

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;

            if (model.IsLockedOut && !user.LockoutEnabled)
            {
                user.LockoutEnabled = true;
            }

            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                // تحديث الأدوار
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

                if (model.SelectedRoles.Any())
                {
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                // تحديث حالة القفل
                if (model.IsLockedOut && model.LockoutEnd.HasValue)
                {
                    await _userManager.SetLockoutEndDateAsync(user, model.LockoutEnd);
                }
                else if (!model.IsLockedOut)
                {
                    await _userManager.SetLockoutEndDateAsync(user, null);
                    await _userManager.ResetAccessFailedCountAsync(user);
                }
            }

            return updateResult;
        }

        public async Task<IdentityResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "المستخدم غير موجود" });

            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityResult> LockUserAsync(string userId, DateTime? lockoutEnd)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "المستخدم غير موجود" });

            return await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
        }

        public async Task<IdentityResult> UnlockUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "المستخدم غير موجود" });

            var result = await _userManager.SetLockoutEndDateAsync(user, null);
            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(user);
            }

            return result;
        }

        public async Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "المستخدم غير موجود" });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return new List<string>();

            var roles = await _userManager.GetRolesAsync(user);
            return roles.ToList();
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return roles ?? new List<string>();
        }
    }
}
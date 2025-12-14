using LoginProject.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LoginProject.Models.ViewModels.Admin;
using LoginProject.Models.ViewModels.Auth;
using LoginProject.Services.Interfaces;

namespace LoginProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly UserManager<ApplicationUser> _userManager;  

        public UsersController(IUserService userService, UserManager<ApplicationUser> userManager)
        {
            _userService = userService;
            _userManager = userManager;  
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string search = "")
        {
            var (users, totalCount) = await _userService.GetUsersAsync(page, pageSize, search);

            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.Search = search;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return View(users);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // GET: Admin/Users/Create
        public async Task<IActionResult> Create()
        {
            var model = new CreateUserViewModel
            {
                AvailableRoles = await _userService.GetAllRolesAsync()
            };
            return View(model);
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _userService.GetAllRolesAsync();
                return View(model);
            }

            var result = await _userService.CreateUserAsync(model);

            if (result.Succeeded)
            {
                TempData["Success"] = "تم إنشاء المستخدم بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableRoles = await _userService.GetAllRolesAsync();
            return View(model);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsLockedOut = user.IsLockedOut,
                LockoutEnd = user.LockoutEnd?.DateTime,
                SelectedRoles = user.Roles,
                AvailableRoles = await _userService.GetAllRolesAsync()
            };

            return View(model);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, EditUserViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                model.AvailableRoles = await _userService.GetAllRolesAsync();
                return View(model);
            }

            var result = await _userService.UpdateUserAsync(model);

            if (result.Succeeded)
            {
                TempData["Success"] = "تم تحديث بيانات المستخدم بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            model.AvailableRoles = await _userService.GetAllRolesAsync();
            return View(model);
        }

        // GET: Admin/Users/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var result = await _userService.DeleteUserAsync(id);

            if (result.Succeeded)
            {
                TempData["Success"] = "تم حذف المستخدم بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "حدث خطأ أثناء حذف المستخدم.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        // POST: Admin/Users/Lock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(string id, int? lockoutHours)
        {
            DateTime? lockoutEnd = null;
            if (lockoutHours.HasValue)
            {
                lockoutEnd = DateTime.UtcNow.AddHours(lockoutHours.Value);
            }

            var result = await _userService.LockUserAsync(id, lockoutEnd);

            if (result.Succeeded)
            {
                var message = lockoutHours.HasValue
                    ? $"تم قفل الحساب لمدة {lockoutHours} ساعة."
                    : "تم قفل الحساب بشكل دائم.";
                TempData["Success"] = message;
            }
            else
            {
                TempData["Error"] = "حدث خطأ أثناء قفل الحساب.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Users/Unlock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var result = await _userService.UnlockUserAsync(id);

            if (result.Succeeded)
            {
                TempData["Success"] = "تم فتح الحساب بنجاح.";
            }
            else
            {
                TempData["Error"] = "حدث خطأ أثناء فتح الحساب.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Users/ResetPassword/5
        public async Task<IActionResult> ResetPassword(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "معرف المستخدم غير صحيح";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Error"] = "المستخدم غير موجود";
                return RedirectToAction(nameof(Index));
            }

            // استخدم AdminResetPasswordViewModel
            var model = new AdminResetPasswordViewModel
            {
                UserId = id,
                Email = user.Email ?? "بريد غير محدد",
                UserName = user.UserName ?? "اسم غير محدد"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    TempData["Error"] = "المستخدم غير موجود";
                    return RedirectToAction(nameof(Index));
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (result.Succeeded)
                {
                    TempData["Success"] = $"تم إعادة تعيين كلمة المرور للمستخدم {user.UserName} بنجاح";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }
    }
}
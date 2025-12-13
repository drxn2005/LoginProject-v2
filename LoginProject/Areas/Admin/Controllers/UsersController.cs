using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetworkCafesControllers.Models.ViewModels.Admin;
using NetworkCafesControllers.Services.Interfaces;

namespace NetworkCafesControllers.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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
                return NotFound();

            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound();

            ViewBag.UserName = user.UserName;
            ViewBag.UserId = user.Id;

            return View();
        }

        // POST: Admin/Users/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string id, string newPassword)
        {
            var result = await _userService.ResetPasswordAsync(id, newPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = "تم إعادة تعيين كلمة المرور بنجاح.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            var user = await _userService.GetUserByIdAsync(id);
            ViewBag.UserName = user?.UserName;
            ViewBag.UserId = id;

            return View();
        }
    }
}
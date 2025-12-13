using Microsoft.AspNetCore.Identity;
using NetworkCafesControllers.Models.Entities;
using NetworkCafesControllers.Models.ViewModels.Admin;

namespace NetworkCafesControllers.Services.Interfaces
{
    public interface IUserService
    {
        Task<(List<UserViewModel> Users, int TotalCount)> GetUsersAsync(int page = 1, int pageSize = 10, string search = "");
        Task<UserViewModel?> GetUserByIdAsync(string userId);
        Task<IdentityResult> CreateUserAsync(CreateUserViewModel model);
        Task<IdentityResult> UpdateUserAsync(EditUserViewModel model);
        Task<IdentityResult> DeleteUserAsync(string userId);
        Task<IdentityResult> LockUserAsync(string userId, DateTime? lockoutEnd);
        Task<IdentityResult> UnlockUserAsync(string userId);
        Task<IdentityResult> ResetPasswordAsync(string userId, string newPassword);
        Task<List<string>> GetUserRolesAsync(string userId);
        Task<List<string>> GetAllRolesAsync();
    }
}
using Microsoft.AspNetCore.Identity;
using NetworkCafesControllers.Models.ViewModels.Auth;

namespace NetworkCafesControllers.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    }
}

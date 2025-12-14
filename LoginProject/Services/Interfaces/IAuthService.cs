using Microsoft.AspNetCore.Identity;
using LoginProject.Models.ViewModels.Auth;

namespace LoginProject.Services.Interfaces
{
    public interface IAuthService
    {
        Task<IdentityResult> RegisterAsync(RegisterViewModel model);
        Task<SignInResult> LoginAsync(LoginViewModel model);
        Task LogoutAsync();
        Task<IdentityResult> ChangePasswordAsync(string userId, ChangePasswordViewModel model);
    }
}

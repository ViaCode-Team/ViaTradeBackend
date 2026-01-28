using Domain.Models;

namespace Application.Intarfaces
{
    public interface IAuthService
    {
        Task<AuthResult> LoginAsync(string login, string password);
        Task LogoutAsync(string refreshToken);
        Task<AuthResult> RegisterAsync(string login, string password);
        Task<AuthResult> RefreshTokenAsync(string refreshToken);
    }
}

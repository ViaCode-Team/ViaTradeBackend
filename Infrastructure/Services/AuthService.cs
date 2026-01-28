using Application.Intarfaces;
using Domain.Entities.DataBase;
using Domain.Models;

namespace Infrastructure.Services
{
    public class AuthService(
        IUserRepository userRepository,
        IJwtHelper jwtHelper,
        ITokenCacheRepository tokenCache) : IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IJwtHelper _jwtHelper = jwtHelper;
        private readonly ITokenCacheRepository _tokenCache = tokenCache;

        private readonly TimeSpan _accessTokenTtl = TimeSpan.FromHours(1);

        public async Task<AuthResult> LoginAsync(string login, string password)
        {
            var user = await _userRepository.GetByLoginAsync(login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.HashPassword))
                throw new UnauthorizedAccessException();

            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.LastLoginDate = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            await _tokenCache.SetAccessTokenAsync(user.Id, accessToken, _accessTokenTtl);

            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResult> RegisterAsync(string login, string password)
        {
            var existingUser = await _userRepository.GetByLoginAsync(login);
            if (existingUser != null)
                throw new InvalidOperationException();

            var user = new User
            {
                Login = login,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(password),
                LastLoginDate = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            var accessToken = _jwtHelper.GenerateAccessToken(user);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            await _userRepository.UpdateRefreshTokenAsync(user.Id, refreshToken);
            await _tokenCache.SetAccessTokenAsync(user.Id, accessToken, _accessTokenTtl);

            return new AuthResult
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResult> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken) 
                ?? throw new UnauthorizedAccessException("Invalid refresh token");
            
            var newAccessToken = _jwtHelper.GenerateAccessToken(user);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.LastLoginDate = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await _tokenCache.SetAccessTokenAsync(user.Id, newAccessToken, _accessTokenTtl);

            return new AuthResult
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

    }
}

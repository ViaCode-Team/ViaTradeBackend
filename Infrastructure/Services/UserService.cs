using System.Security.Cryptography;
using Application.Interfaces;
using Domain.Entities.DataBase;
using Domain.Entities.Redis;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositories.Redis;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services
{
    public class UserService(
        UserRepository userRepository,
        TgTokenRepository tgTokenRepository,
        ILogger<UserService> logger) : IUserService
    {
        private readonly UserRepository _userRepository = userRepository;
        private readonly TgTokenRepository _tgTokenRepository = tgTokenRepository;
        private readonly ILogger<UserService> _logger = logger;

        public async Task<User> EnsureUserAsync(int userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Ensuring user exists with ID: {UserId}", userId);

            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User not found with ID: {UserId}", userId);
                throw new UnauthorizedAccessException();
            }

            _logger.LogInformation("User verified successfully: {UserId}", userId);
            return user;
        }

        public async Task<string> GenerateTgLink(int userId)
        {
            _logger.LogInformation("Generating Telegram link for user: {UserId}", userId);

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(24))
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            var entity = new TgTokenEntity
            {
                Id = token,
                UserId = userId
            };

            await _tgTokenRepository.SetAsync(entity, TimeSpan.FromMinutes(5));
            _logger.LogInformation("Telegram token generated and stored for user: {UserId}", userId);

            return $"https://t.me/ViaTradeBot?start={token}";
        }

        public async Task<int?> GetUserId(string tgToken)
        {
            _logger.LogInformation("Getting user ID for Telegram token");

            var entity = await _tgTokenRepository.GetAsync(tgToken);

            if (entity == null)
            {
                _logger.LogWarning("No user found for Telegram token");
                return null;
            }

            await _tgTokenRepository.RemoveAsync(tgToken);
            _logger.LogInformation("Telegram token used and removed for user: {UserId}", entity.UserId);

            return entity.UserId;
        }

        public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting user by login: {Login}", login);
            return await _userRepository.GetByLoginAsync(login, cancellationToken);
        }

        public async Task UpdateLastLoginDateAsync(int userId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating last login date for user: {UserId}", userId);
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null)
            {
                user.LastLoginDate = DateTime.UtcNow;
                await _userRepository.SaveChangesAsync();
            }
        }
    }
}

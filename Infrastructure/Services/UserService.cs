using System.Security.Cryptography;
using Application.Interfaces;
using Domain.Entities.DataBase;
using Domain.Entities.Redis;
using Infrastructure.Repositories.Redis;
using Infrastructure.Repositoryes.DataBase;

namespace Infrastructure.Services
{
    public class UserService(UserRepository userRepository, TgTokenRepository tgTokenRepository) : IUserService
    {
        private readonly UserRepository _userRepository = userRepository;
        private readonly TgTokenRepository _tgTokenRepository = tgTokenRepository;

        public async Task<User> EnsureUserAsync(int userId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetByIdAsync(userId, cancellationToken)
                ?? throw new UnauthorizedAccessException();
        }

        public async Task<string> GenerateTgLink(int userId)
        {
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

            return $"https://t.me/ViaTradeBot?start={token}";
        }

        public async Task<int?> GetUserId(string tgToken)
        {
            var entity = await _tgTokenRepository.GetAsync(tgToken);

            if (entity == null) return null;

            await _tgTokenRepository.RemoveAsync(tgToken);

            return entity.UserId;
        }

    }
}

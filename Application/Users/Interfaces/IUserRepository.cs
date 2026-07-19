using Application.Common.Interfaces.Repositories;
using Domain.Users.Entities;

namespace Application.Users.Interfaces;

public interface IUserRepository : IRepository<User>
{
	Task<User?> GetByLoginAsync(string login, CancellationToken ct = default);
	Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken ct = default);
	Task<int> UpdateTelegramIdAsync(int userId, string telegramId, CancellationToken ct = default);
	Task<int> UpdateLastLoginDateAsync(int userId, DateTime lastLoginDate, CancellationToken ct = default);
}

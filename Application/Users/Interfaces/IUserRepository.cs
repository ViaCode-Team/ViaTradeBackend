using Application.Common.Interfaces.Repositories;
using Application.Users.Models;
using Domain.Entities;

namespace Application.Users.Interfaces;

public interface IUserRepository : IRepository<User>
{
	Task<UserLoginDto?> FindLoginUserAsync(string login, CancellationToken ct = default);
	Task<UserTokenDto?> FindTokenUserAsync(int userId, CancellationToken ct = default);
	Task<UserMeDto?> FindMeAsync(int userId, CancellationToken ct = default);
	Task<IReadOnlyList<UserTelegramDto>> ListTelegramRecipientsAsync(CancellationToken ct = default);
	Task<int> UpdateTelegramIdAsync(int userId, string telegramId, CancellationToken ct = default);
	Task<int> UpdateLastLoginAtAsync(int userId, DateTime lastLoginDate, CancellationToken ct = default);
}

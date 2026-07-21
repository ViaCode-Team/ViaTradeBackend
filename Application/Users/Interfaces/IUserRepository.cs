using Application.Common.Interfaces.Repositories;
using Application.Users.Models;
using Domain.Users.Entities;

namespace Application.Users.Interfaces;

public interface IUserRepository : IRepository<User>
{
	Task<UserLoginDto?> GetLoginUserAsync(string login, CancellationToken ct = default);
	Task<UserTokenDto?> GetTokenUserAsync(int userId, CancellationToken ct = default);
	Task<UserMeDto?> GetMeAsync(int userId, CancellationToken ct = default);
	Task<IReadOnlyList<UserTelegramDto>> GetTelegramRecipientsAsync(CancellationToken ct = default);
	Task<int> UpdateTelegramIdAsync(int userId, string telegramId, CancellationToken ct = default);
	Task<int> UpdateLastLoginDateAsync(int userId, DateTime lastLoginDate, CancellationToken ct = default);
}

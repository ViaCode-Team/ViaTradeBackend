namespace ViaTrade.Application.Users.Common.Abstractions;

public interface IUserRepository
{
	Task<int> ExecuteUpdateTelegramIdAsync(int userId, string telegramId, CancellationToken ct = default);
	Task<int> ExecuteUpdateLastLoginAtAsync(int userId, DateTime lastLoginDate, CancellationToken ct = default);
}

using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Users.Interfaces;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class UserEfRepository(AppDbContext context) : IUserRepository
{
	public Task<int> ExecuteUpdateTelegramIdAsync(int userId, string telegramId, CancellationToken ct)
	{
		return context
			.Users.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.TelegramId, telegramId), ct);
	}

	public async Task<int> ExecuteUpdateLastLoginAtAsync(int userId, DateTime lastLoginDate, CancellationToken ct)
	{
		return await context
			.Users.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginAt, lastLoginDate), ct);
	}
}

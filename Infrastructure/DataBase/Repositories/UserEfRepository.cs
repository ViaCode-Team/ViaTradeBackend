using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Users.Interfaces;
using ViaTrade.Application.Users.Models;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class UserEfRepository(AppDbContext context, EfQueryObjectBuilder queryObjectBuilder)
	: BaseEfRepository<User>(context, queryObjectBuilder),
		IUserRepository
{
	public async Task<UserLoginDto?> FindLoginUserAsync(string login, CancellationToken ct)
	{
		return await _dbSet
			.Where(user => user.Login == login)
			.Select(user => new UserLoginDto(user.Id, user.Login, user.PasswordHash))
			.FirstOrDefaultAsync(ct);
	}

	public async Task<UserMeDto?> FindMeAsync(int userId, CancellationToken ct)
	{
		return await _dbSet
			.Where(user => user.Id == userId)
			.Select(user => new UserMeDto
			{
				Id = user.Id,
				Login = user.Login,
				LastLoginAt = user.LastLoginAt,
				RegisteredAt = user.RegisteredAt,
				TelegramId = user.TelegramId,
			})
			.FirstOrDefaultAsync(ct);
	}

	public async Task<UserTokenDto?> FindTokenUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet
			.Where(user => user.Id == userId)
			.Select(user => new UserTokenDto(user.Id, user.Login))
			.FirstOrDefaultAsync(ct);
	}

	public Task<int> UpdateTelegramIdAsync(int userId, string telegramId, CancellationToken ct)
	{
		return _dbSet
			.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.TelegramId, telegramId), ct);
	}

	public async Task<int> UpdateLastLoginAtAsync(int userId, DateTime lastLoginDate, CancellationToken ct)
	{
		return await _dbSet
			.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginAt, lastLoginDate), ct);
	}
}

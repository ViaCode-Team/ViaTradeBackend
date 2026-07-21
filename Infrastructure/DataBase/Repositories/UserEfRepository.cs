using Application.Users.Interfaces;
using Application.Users.Models;
using Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class UserEfRepository(AppDbContext context) : GenericEfRepository<User>(context), IUserRepository
{
	public async Task<UserLoginDto?> FindLoginUserAsync(string login, CancellationToken ct)
	{
		return await _dbSet
			.Where(user => user.Login == login)
			.Select(user => new UserLoginDto(user.Id, user.Login, user.HashPassword))
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
				LastLoginDate = user.LastLoginDate,
				RegisterDate = user.RegisterDate,
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

	public async Task<IReadOnlyList<UserTelegramDto>> ListTelegramRecipientsAsync(CancellationToken ct)
	{
		return await _dbSet
			.Where(user => user.TelegramId != null)
			.Select(user => new UserTelegramDto { Id = user.Id, TelegramId = user.TelegramId! })
			.ToListAsync(ct);
	}

	public async Task<int> UpdateTelegramIdAsync(int userId, string telegramId, CancellationToken ct)
	{
		return await _dbSet
			.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.TelegramId, telegramId), ct);
	}

	public async Task<int> UpdateLastLoginDateAsync(int userId, DateTime lastLoginDate, CancellationToken ct)
	{
		return await EfDatabaseOperation.ExecuteAsync(() =>
			_dbSet
				.Where(u => u.Id == userId)
				.ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginDate, lastLoginDate), ct)
		);
	}
}

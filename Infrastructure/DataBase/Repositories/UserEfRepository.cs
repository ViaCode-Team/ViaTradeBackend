using Application.Users.Interfaces;
using Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class UserEfRepository(AppDbContext context) : GenericEfRepository<User>(context), IUserRepository
{
	public async Task<User?> GetByLoginAsync(string login, CancellationToken ct)
	{
		return await _dbSet.FirstOrDefaultAsync(u => u.Login == login, ct);
	}

	public async Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken ct)
	{
		return await _dbSet.Where(u => u.TelegramId != null).ToListAsync(ct);
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

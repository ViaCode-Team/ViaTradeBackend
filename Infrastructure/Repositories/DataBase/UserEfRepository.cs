using Application.Users.Interfaces;
using Domain.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class UserEfRepository(AppDbContext context) : GenericEfRepository<User>(context), IUserRepository
{
	public async Task<User?> GetByLoginAsync(string login, CancellationToken ct = default)
	{
		return await _dbSet.FirstOrDefaultAsync(u => u.Login == login, ct);
	}

	public async Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken ct = default)
	{
		return await _dbSet.Where(u => u.TgId != null).ToListAsync(ct);
	}

	public async Task<int> UpdateTgIdAsync(int userId, string tgId, CancellationToken ct = default)
	{
		return await _dbSet
			.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.TgId, tgId), ct);
	}

	public async Task<int> UpdateLastLoginDateAsync(int userId, DateTime lastLoginDate, CancellationToken ct = default)
	{
		return await _dbSet
			.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginDate, lastLoginDate), ct);
	}
}

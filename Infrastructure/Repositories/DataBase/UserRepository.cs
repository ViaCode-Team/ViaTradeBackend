using Domain.Users.Entities;
using Application.Interfaces.Repositories.Database;
using Application.Models;
using Domain.Entities.DataBase;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.DataBase;

public class UserRepository(AppDbContext context) : GenericRepository<User>(context), IUserRepository
{
	public async Task<User?> GetByLoginAsync(string login, CancellationToken cancellationToken = default)
	{
		return await _dbSet.FirstOrDefaultAsync(u => u.Login == login, cancellationToken);
	}

	public async Task<IEnumerable<User>> GetAllWithTgLinkAsync(CancellationToken cancellationToken = default)
	{
		return await _dbSet.Where(u => u.TgId != null).ToListAsync(cancellationToken);
	}

	public async Task<int> UpdateTgIdAsync(int userId, string tgId, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.TgId, tgId), cancellationToken);
	}

	public async Task<int> UpdateLastLoginDateAsync(int userId, DateTime lastLoginDate, CancellationToken cancellationToken = default)
	{
		return await _dbSet
			.Where(u => u.Id == userId)
			.ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginDate, lastLoginDate), cancellationToken);
	}
}

using Application.Common.Models;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;

namespace Infrastructure.DataBase.Repositories;

public class UserStrategyTradeCodeEfRepository(AppDbContext context)
	: GenericEfRepository<UserStrategyTradeCode>(context),
		IUserStrategyTradeCodeRepository
{
	public async Task<PageResult<UserStrategyTradeCode>> GetPagedAsync(
		int userId,
		PageOptions page,
		CancellationToken ct
	)
	{
		return await FindPagedAsync(e => e.UserId == userId, page, ct);
	}
}

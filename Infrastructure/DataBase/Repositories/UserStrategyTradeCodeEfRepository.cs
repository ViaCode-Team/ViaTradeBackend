using Application.Common.Models;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;

namespace Infrastructure.DataBase.Repositories;

public class UserStrategyTradeCodeEfRepository(AppDbContext context)
	: GenericEfRepository<UserStrategyTradeCode>(context),
		IUserStrategyTradeCodeRepository
{
	public async Task<PageResult<UserStrategyTradeCode>> GetPageByUserAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		return await GetPageByAsync(strategyCode => strategyCode.UserId == userId, pageOptions, ct);
	}
}

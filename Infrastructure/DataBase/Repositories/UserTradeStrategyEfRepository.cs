using Application.Common.Models;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;
using Infrastructure.Utils;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class UserTradeStrategyEfRepository(AppDbContext context)
	: GenericEfRepository<UserTradeStrategy>(context),
		IUserTradeStrategyRepository
{
	public async Task<int> CountByUserAsync(int userId, CancellationToken ct)
	{
		return await _dbSet.CountAsync(e => e.UserId == userId, ct);
	}

	public async Task<PageResult<UserTradeStrategy>> GetByUserPagedAsync(
		int userId,
		PageOptions page,
		CancellationToken ct
	)
	{
		return await FindPagedAsync(strategy => strategy.UserId == userId, page, ct);
	}

	public async Task<Dictionary<string, List<string>>> GetUserPreferencesAsync(int userId, CancellationToken ct)
	{
		var userCodesQuery = _context.GetUserCodesQuery(userId);
		var allowedStrategiesQuery = _context.GetAllowedStrategiesQuery(userId);

		var projectedQuery = userCodesQuery
			.FilterByAllowedStrategies(allowedStrategiesQuery)
			.ProjectToStrategyAndTradeCode();

		var queryResults = await projectedQuery.ToListAsync(ct);

		return queryResults.GroupByStrategyName();
	}
}

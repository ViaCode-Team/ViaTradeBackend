using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Domain.Strategies.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class TradeStrategyEfRepository(AppDbContext context)
	: GenericEfRepository<TradeStrategy>(context),
		ITradeStrategyRepository
{
	public async Task<StrategyCountsDto> GetStatisticAsync(int userId, CancellationToken ct)
	{
		var query = _context
			.Users.Where(user => user.Id == userId)
			.Select(_ => new StrategyCountsDto(
				_context.TradeStrategies.LongCount(),
				_context.UserTradeStrategies.LongCount(link => link.UserId == userId)
			));

		return await query.SingleOrDefaultAsync(ct)
			?? throw new KeyNotFoundException($"User with ID {userId} was not found.");
	}

	public async Task<Dictionary<string, int?>> GetAccuracyMapAsync(CancellationToken ct)
	{
		var strategies = await _dbSet
			.Select(tradeStrategy => new { tradeStrategy.Name, tradeStrategy.Accuracy })
			.ToListAsync(ct);

		return strategies.ToDictionary(tradeStrategy => tradeStrategy.Name, tradeStrategy => tradeStrategy.Accuracy);
	}

	public async Task<int?> GetAccuracyByNameAsync(string name, CancellationToken ct)
	{
		return await _dbSet
			.Where(tradeStrategy => tradeStrategy.Name == name)
			.Select(tradeStrategy => tradeStrategy.Accuracy)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TradeStrategy>> GetPagedFilteredAsync(
		int userId,
		IQuerySpecification<TradeStrategy> spec,
		PageOptions page,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet, spec);
		if (spec.SortExpressions.Count == 0)
			query = query.OrderBy(strategy => strategy.Id);

		var pagedStrategies = await query
			.Select(strategy => new
			{
				Strategy = strategy,
				IsActive = strategy.UserTradeStrategies.Any(link => link.UserId == userId),
			})
			.ToPagedAsync(page, ct);

		return pagedStrategies.Map(strategy =>
		{
			strategy.Strategy.IsActive = strategy.IsActive;
			return strategy.Strategy;
		});
	}
}

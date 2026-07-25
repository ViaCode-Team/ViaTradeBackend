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
	public async Task<StrategyCountsDto?> FindStatisticsAsync(int userId, CancellationToken ct)
	{
		var query = _context
			.Users.Where(user => user.Id == userId)
			.Select(_ => new StrategyCountsDto(
				_context.TradeStrategies.LongCount(),
				_context.UserTradeStrategies.LongCount(link => link.UserId == userId)
			));

		return await query.SingleOrDefaultAsync(ct);
	}

	public async Task<Dictionary<string, int?>> GetAccuracyMapAsync(CancellationToken ct)
	{
		var strategies = await _dbSet
			.Select(tradeStrategy => new { tradeStrategy.Name, tradeStrategy.Accuracy })
			.ToListAsync(ct);

		return strategies.ToDictionary(tradeStrategy => tradeStrategy.Name, tradeStrategy => tradeStrategy.Accuracy);
	}

	public async Task<int?> FindAccuracyByNameAsync(string name, CancellationToken ct)
	{
		return await _dbSet
			.Where(tradeStrategy => tradeStrategy.Name == name)
			.Select(tradeStrategy => tradeStrategy.Accuracy)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<RelatedTradeStrategyDto?> FindByNameAsync(int userId, string name, CancellationToken ct)
	{
		return await _dbSet
			.Where(strategy => strategy.Name == name)
			.Select(strategy => new RelatedTradeStrategyDto(
				strategy.Id,
				strategy.Name,
				strategy.Description,
				strategy.Accuracy,
				strategy.SignalFrequency,
				strategy.InvestmentHorizon,
				strategy.LogicDesc,
				strategy.UseDesc,
				strategy.LimitDesc,
				strategy.UserTradeStrategies.Any(link => link.UserId == userId)
			))
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<TradeStrategy>> GetPageAsync(
		int userId,
		IQuerySpecification<TradeStrategy> spec,
		PageOptions pageOptions,
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
			.ToPagedAsync(pageOptions, ct);

		return pagedStrategies.Map(strategy =>
		{
			strategy.Strategy.IsActive = strategy.IsActive;
			return strategy.Strategy;
		});
	}
}

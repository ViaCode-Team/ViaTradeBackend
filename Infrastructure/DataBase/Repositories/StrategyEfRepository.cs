using Application.Common.Interfaces;
using Application.Common.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Domain.Entities;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataBase.Repositories;

public class StrategyEfRepository(AppDbContext context) : BaseEfRepository<Strategy>(context), IStrategyRepository
{
	public async Task<StrategyCountsDto?> FindStatisticsAsync(int userId, CancellationToken ct)
	{
		var query = _context
			.Users.Where(user => user.Id == userId)
			.Select(_ => new StrategyCountsDto(
				_context.Strategies.LongCount(),
				_context.UserStrategies.LongCount(link => link.UserId == userId)
			));

		return await query.SingleOrDefaultAsync(ct);
	}

	public async Task<Dictionary<string, int?>> GetAccuracyMapAsync(CancellationToken ct)
	{
		var strategies = await _dbSet.Select(strategy => new { strategy.Name, strategy.Accuracy }).ToListAsync(ct);

		return strategies.ToDictionary(strategy => strategy.Name, strategy => strategy.Accuracy);
	}

	public async Task<Strategy?> FindWithActivityAsync(int userId, int strategyId, CancellationToken ct)
	{
		var result = await _dbSet
			.Where(strategy => strategy.Id == strategyId)
			.Select(strategy => new
			{
				Strategy = strategy,
				IsActive = strategy.UserStrategies.Any(link => link.UserId == userId),
			})
			.FirstOrDefaultAsync(ct);
		if (result == null)
			return null;

		result.Strategy.IsActive = result.IsActive;
		return result.Strategy;
	}

	public async Task<StrategyInstrumentLinkState?> FindInstrumentLinkStateAsync(
		int userId,
		int strategyId,
		int instrumentId,
		CancellationToken ct
	)
	{
		return await _dbSet
			.Where(strategy => strategy.Id == strategyId)
			.Select(_ => new StrategyInstrumentLinkState(
				_context.Instruments.Any(instrument => instrument.Id == instrumentId),
				_context.UserStrategyInstruments.Any(link =>
					link.UserId == userId && link.StrategyId == strategyId && link.InstrumentId == instrumentId
				)
			))
			.SingleOrDefaultAsync(ct);
	}

	public async Task<int?> FindAccuracyByNameAsync(string name, CancellationToken ct)
	{
		return await _dbSet
			.Where(strategy => strategy.Name == name)
			.Select(strategy => strategy.Accuracy)
			.FirstOrDefaultAsync(ct);
	}

	public async Task<PageResult<Strategy>> GetPageAsync(
		int userId,
		IQuerySpecification<Strategy> spec,
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
				IsActive = strategy.UserStrategies.Any(link => link.UserId == userId),
			})
			.ToPagedAsync(pageOptions, ct);

		return pagedStrategies.Map(strategy =>
		{
			strategy.Strategy.IsActive = strategy.IsActive;
			return strategy.Strategy;
		});
	}

	public async Task<PageResult<Strategy>> GetPageSearchAsync(
		ISearchSpecification<Strategy> specification,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = specification.Apply(_dbSet)
			.OrderBy(trade => trade.Id);

		return await query.ToPagedAsync(pageOptions, ct);
	}
}

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
				_context.Strategies.LongCount(strategy => strategy.UserStrategies.Any(link => link.UserId == userId))
			));

		return await query.SingleOrDefaultAsync(ct);
	}

	public async Task<Dictionary<string, int?>> GetAccuracyMapAsync(CancellationToken ct)
	{
		var strategies = await _dbSet.Select(strategy => new { strategy.Name, strategy.Accuracy }).ToListAsync(ct);

		return strategies.ToDictionary(strategy => strategy.Name, strategy => strategy.Accuracy);
	}

	public async Task<StrategySubscriptionDto?> FindSubscriptionAsync(int userId, int strategyId, CancellationToken ct)
	{
		return await _dbSet
			.Where(strategy => strategy.Id == strategyId)
			.WithSubscriptionState(userId)
			.FirstOrDefaultAsync(ct);
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

	public async Task<PageResult<StrategySubscriptionDto>> GetPageAsync(
		int userId,
		IQuerySpecification<Strategy> spec,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var query = SpecificationEvaluator.GetQuery(_dbSet, spec);
		if (spec.SortExpressions.Count == 0)
			query = query.OrderBy(strategy => strategy.Id);

		return await query.WithSubscriptionState(userId).ToPagedAsync(pageOptions, ct);
	}
}

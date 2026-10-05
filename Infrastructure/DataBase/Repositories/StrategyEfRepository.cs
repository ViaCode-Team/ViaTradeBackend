using Microsoft.EntityFrameworkCore;
using ViaTrade.Application.Strategies.Common.Abstractions;
using ViaTrade.Application.Strategies.GetStatistics;
using ViaTrade.Application.Strategies.LinkInstrument;

namespace ViaTrade.Infrastructure.DataBase.Repositories;

public class StrategyEfRepository(AppDbContext context) : IStrategyRepository
{
	public async Task<StrategyCounts?> FindStatisticsAsync(int userId, CancellationToken ct)
	{
		var query = context
			.Users.Where(user => user.Id == userId)
			.Select(_ => new StrategyCounts(
				context.Strategies.LongCount(),
				context.Strategies.LongCount(strategy => strategy.UserStrategies.Any(link => link.UserId == userId))
			));

		return await query.SingleOrDefaultAsync(ct);
	}

	public async Task<StrategyInstrumentLinkState?> FindInstrumentLinkStateAsync(
		int userId,
		int strategyId,
		int instrumentId,
		CancellationToken ct
	)
	{
		return await context
			.Strategies.Where(strategy => strategy.Id == strategyId)
			.Select(_ => new StrategyInstrumentLinkState(
				context.Instruments.Any(instrument => instrument.Id == instrumentId),
				context.UserStrategyInstruments.Any(link =>
					link.UserId == userId && link.StrategyId == strategyId && link.InstrumentId == instrumentId
				)
			))
			.SingleOrDefaultAsync(ct);
	}
}

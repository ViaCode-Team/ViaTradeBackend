using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Interfaces.Repositories;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Common.Queries;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Application.Strategies.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies;

public class StrategyQueryService(
	IReadRepository<Instrument> instrumentRepository,
	IReadRepository<Strategy> strategyRepository,
	IReadRepository<UserStrategyInstrument> userStrategyInstrumentRepository,
	IStrategyRepository strategyStatistics
) : IStrategyQueryService
{
	public async Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var counts = await strategyStatistics.FindStatisticsAsync(userId, ct);
		if (counts == null)
			throw new NotFoundException("User not found.", "user_not_found");

		long unsubscribedStrategiesCount = counts.TotalStrategiesCount - counts.SubscribedStrategiesCount;
		if (unsubscribedStrategiesCount < 0)
		{
			throw new DataIntegrityException(
				$"Subscribed strategy count exceeds total strategy count. "
					+ $"UserId={userId}, "
					+ $"Total={counts.TotalStrategiesCount}, "
					+ $"Subscribed={counts.SubscribedStrategiesCount}."
			);
		}

		return new StrategyStatisticDto(
			counts.TotalStrategiesCount,
			counts.SubscribedStrategiesCount,
			unsubscribedStrategiesCount
		);
	}

	public async Task<PageResult<StrategySubscriptionDto>> GetPageAsync(
		int userId,
		StrategyFilter strategyFilter,
		StrategySearch strategySearch,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var specification = new StrategiesPageSpecification(strategyFilter, strategySearch, pageOptions, strategySort);
		return await PageQuery.ExecuteAsync(
			strategyRepository,
			specification,
			strategy => new StrategySubscriptionDto(
				strategy,
				strategy.UserStrategies.Any(link => link.UserId == userId)
			),
			ct
		);
	}

	public async Task<StrategySubscriptionDto> GetAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategy = await strategyRepository.FirstOrDefaultAsync(
			strategy => strategy.Id == strategyId,
			strategy => new StrategySubscriptionDto(
				strategy,
				strategy.UserStrategies.Any(link => link.UserId == userId)
			),
			ct
		);
		if (strategy == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		return strategy;
	}

	public async Task<PageResult<StrategySubscriptionDto>> GetPageByInstrumentAsync(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var instrumentExists = await instrumentRepository.AnyAsync(instrument => instrument.Id == instrumentId, ct);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		var specification = new InstrumentStrategiesPageSpecification(
			userId,
			instrumentId,
			strategyFilter,
			pageOptions,
			strategySort
		);
		return await PageQuery.ExecuteAsync(
			userStrategyInstrumentRepository,
			specification,
			link => new StrategySubscriptionDto(
				link.Strategy!,
				link.Strategy!.UserStrategies.Any(subscription => subscription.UserId == userId)
			),
			ct
		);
	}

	public async Task<PageResult<RelatedInstrumentDto>> GetInstrumentsByStrategyPageAsync(
		int userId,
		int strategyId,
		StrategyInstrumentFilter instrumentFilter,
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var strategyExists = await strategyRepository.AnyAsync(strategy => strategy.Id == strategyId, ct);
		if (!strategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		var specification = new StrategyInstrumentsPageSpecification(
			userId,
			strategyId,
			instrumentFilter,
			pageOptions,
			instrumentSort
		);
		return await PageQuery.ExecuteAsync(
			userStrategyInstrumentRepository,
			specification,
			link => new RelatedInstrumentDto(link.Instrument!.Id, link.Instrument.Symbol, link.Instrument.Description),
			ct
		);
	}
}

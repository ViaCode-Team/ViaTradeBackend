using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Notes.Models;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Application.Strategies.Models;
using ViaTrade.Application.Strategies.Specifications;
using ViaTrade.Domain.Entities;

namespace ViaTrade.Application.Strategies;

public class StrategyQueryService(
	IInstrumentRepository instrumentRepository,
	IStrategyRepository strategyRepository,
	IUserStrategyInstrumentRepository userStrategyInstrumentRepository
) : IStrategyQueryService
{
	public async Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var counts = await strategyRepository.FindStatisticsAsync(userId, ct);
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
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new StrategyQuerySpecification(strategyFilter, strategySort);
		return await strategyRepository.GetPageAsync(userId, spec, pageOptions, ct);
	}

	public async Task<PageResult<StrategySubscriptionDto>> GetPageSearchAsync(
		int userId,
		SearchFilter strategyFilter,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new StrategySearchSpecification(strategyFilter);
		return await strategyRepository.GetPageSearchAsync(userId, spec, pageOptions, ct);
	}

	public async Task<StrategySubscriptionDto> GetAsync(int userId, int strategyId, CancellationToken ct)
	{
		var strategy = await strategyRepository.FindSubscriptionAsync(userId, strategyId, ct);
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
		var instrumentExists = await instrumentRepository.ExistsAsync(instrument => instrument.Id == instrumentId, ct);

		if (!instrumentExists)
			throw new NotFoundException("Instrument not found.", "instrument_not_found");

		return await userStrategyInstrumentRepository.GetStrategiesPageByInstrumentAsync(
			userId,
			instrumentId,
			strategyFilter,
			strategySort,
			pageOptions,
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
		var result = await userStrategyInstrumentRepository.GetInstrumentsPageByStrategyAsync(
			userId,
			strategyId,
			instrumentFilter,
			instrumentSort,
			pageOptions,
			ct
		);

		if (!result.StrategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		return result.Page;
	}
}

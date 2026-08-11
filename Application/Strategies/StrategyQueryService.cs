using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Instruments.Models;
using Application.Notes.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Application.Strategies.Specifications;

namespace Application.Strategies;

public class StrategyQueryService(
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
		InstrumentSort instrumentSort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var result = await userStrategyInstrumentRepository.GetInstrumentsPageByStrategyAsync(
			userId,
			strategyId,
			instrumentSort,
			pageOptions,
			ct
		);

		if (!result.StrategyExists)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		return result.Page;
	}
}

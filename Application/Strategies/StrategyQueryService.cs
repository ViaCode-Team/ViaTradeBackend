using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Specifications;
using Application.Notes.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Application.TradeCodes.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies;

public class StrategyQueryService(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository
) : IStrategyQueryService
{
	public async Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var counts = await tradeStrategyRepository.FindStatisticsAsync(userId, ct);
		if (counts == null)
			throw new NotFoundException("User not found.", "user_not_found");

		long notLinkedStratagiesCount = counts.TotalStrategiesCount - counts.ActiveStrategiesCount;
		if (notLinkedStratagiesCount < 0)
		{
			throw new DataIntegrityException(
				$"Active strategy count exceeds total strategy count. "
					+ $"UserId={userId}, "
					+ $"Total={counts.TotalStrategiesCount}, "
					+ $"Active={counts.ActiveStrategiesCount}."
			);
		}

		return new StrategyStatisticDto(
			counts.TotalStrategiesCount,
			counts.ActiveStrategiesCount,
			notLinkedStratagiesCount
		);
	}

	public async Task<PageResult<TradeStrategy>> GetPageAsync(
		int userId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var spec = new StrategyQuerySpecification(userId, strategyFilter, strategySort);
		return await tradeStrategyRepository.GetPageAsync(userId, spec, pageOptions, ct);
	}

	public async Task<TradeStrategy> GetAsync(int userId, int strategyId, CancellationToken ct)
	{
		var tradeStrategy = await tradeStrategyRepository.FindForUserAsync(userId, strategyId, ct);
		if (tradeStrategy == null)
			throw new NotFoundException("Strategy not found.", "strategy_not_found");

		return tradeStrategy;
	}

	public async Task<PageResult<TradeStrategy>> GetPageByInstrumentAsync(
		int userId,
		int instrumentId,
		StrategyFilter strategyFilter,
		StrategySort strategySort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		return await userStrategyTradeCodeRepository.GetStrategiesPageByInstrumentAsync(
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
		return await userStrategyTradeCodeRepository.GetInstrumentsPageByStrategyAsync(
			userId,
			strategyId,
			instrumentSort,
			pageOptions,
			ct
		);
	}
}

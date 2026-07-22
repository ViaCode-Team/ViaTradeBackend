using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Common.Specifications;
using Application.Notes.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Models;
using Domain.Strategies.Entities;

namespace Application.Strategies;

public class StrategyQueryService(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository,
	IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository
) : IStrategyQueryService
{
	public async Task<StrategyStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var counts = await tradeStrategyRepository.GetStatisticsAsync(userId, ct);
		if (counts == null)
			throw new KeyNotFoundException($"User with ID {userId} was not found for get stratagy statiscs.");

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

	public async Task<TradeStrategy> GetAsync(int strategyId, CancellationToken ct)
	{
		var tradeStrategy = await tradeStrategyRepository.FindByIdAsync(strategyId, ct);
		if (tradeStrategy == null)
			throw new KeyNotFoundException();

		return tradeStrategy;
	}

	public async Task<PageResult<UserTradeStrategy>> GetUserStrategiesPageAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		return await userTradeStrategyRepository.GetPageByUserAsync(userId, pageOptions, ct);
	}

	public async Task<PageResult<UserStrategyTradeCode>> GetUserStrategyTradeCodesPageAsync(
		int userId,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		return await userStrategyTradeCodeRepository.GetPageByUserAsync(userId, pageOptions, ct);
	}
}

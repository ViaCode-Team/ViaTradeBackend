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
		var counts = await tradeStrategyRepository.GetStatisticAsync(userId, ct);
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

	public async Task<PageResult<TradeStrategy>> GetAsync(
		int userId,
		StrategyFilter filter,
		StrategySort sort,
		PageOptions page,
		CancellationToken ct
	)
	{
		var spec = new StrategyQuerySpecification(userId, filter, sort);
		return await tradeStrategyRepository.GetPagedFilteredAsync(userId, spec, page, ct);
	}

	public async Task<TradeStrategy> GetAsync(int strategyId, CancellationToken ct)
	{
		return await tradeStrategyRepository.GetByIdAsync(strategyId, ct) ?? throw new KeyNotFoundException();
	}

	public async Task<PageResult<UserTradeStrategy>> GetUserLinkedAsync(
		int userId,
		PageOptions page,
		CancellationToken ct
	)
	{
		return await userTradeStrategyRepository.GetByUserPagedAsync(userId, page, ct);
	}

	public async Task<PageResult<UserStrategyTradeCode>> GetUserLinkedCodesAsync(
		int userId,
		PageOptions page,
		CancellationToken ct
	)
	{
		return await userStrategyTradeCodeRepository.GetPagedAsync(userId, page, ct);
	}
}

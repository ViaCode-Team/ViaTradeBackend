using Application.Common.Queries;
using Application.Common.Specifications;
using Application.Statistics.Models;
using Application.Strategies.Interfaces;
using Application.Strategies.Queries;
using Domain.Strategies.Entities;

namespace Application.Strategies;

public class StrategyQueryService(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository,
	IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository
) : IStrategyQueryService
{
	public async Task<StrategyStatisticReadModel> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var totalStrategiesTask = tradeStrategyRepository.CountAsync(ct);
		var activeStrategiesTask = userTradeStrategyRepository.CountByUserAsync(userId, ct);

		await Task.WhenAll(totalStrategiesTask, activeStrategiesTask);

		var totalStrategies = await totalStrategiesTask;
		var activeStrategies = await activeStrategiesTask;

		return new StrategyStatisticReadModel
		{
			TotalStrategies = totalStrategies,
			ActiveStrategies = activeStrategies,
			DisabledStrategies = Math.Max(totalStrategies - activeStrategies, 0),
		};
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

using Application.Common.Models.Filters;
using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Common.Specifications;
using Application.Statistics.Models;
using Application.Strategies.Interfaces;
using Domain.Strategies.Entities;

namespace Application.Strategies;

public class StrategyQueryService(
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository,
	IUserStrategyTradeCodeRepository userStrategyTradeCodeRepository) : IStrategyQueryService
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
			DisabledStrategies = Math.Max(totalStrategies - activeStrategies, 0)
		};
	}

	public async Task<PagedResult<TradeStrategy>> GetAsync(int userId, StrategyFilterRequest? filterRequest, StrategySortRequest? sortRequest, PaginationRequest? paginationRequest, CancellationToken ct)
	{
		var spec = new StrategyQuerySpecification(userId, filterRequest, sortRequest);
		return await tradeStrategyRepository.GetPagedFilteredAsync(userId, spec, paginationRequest, ct);
	}

	public async Task<TradeStrategy> GetAsync(int strategyId, CancellationToken ct)
	{
		return await tradeStrategyRepository.GetByIdAsync(strategyId, ct)
			?? throw new KeyNotFoundException();
	}

	public async Task<PagedResult<UserTradeStrategy>> GetUserLinkedAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct)
	{
		return await userTradeStrategyRepository.GetByUserPagedAsync(userId, paginationRequest, ct);
	}

	public async Task<PagedResult<UserStrategyTradeCode>> GetUserLinkedCodesAsync(int userId, PaginationRequest paginationRequest, CancellationToken ct)
	{
		return await userStrategyTradeCodeRepository.GetPagedAsync(userId, paginationRequest, ct);
	}
}

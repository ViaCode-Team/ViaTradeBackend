using Domain.Trades.Enums;
using Domain.Trades.Entities;
using Application.Contracts.Dto.Requests.Trade;
using Application.Models.Statistic;
using Domain.Entities.DataBase;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Models.Sort;

namespace Application.Interfaces;

public interface IStrategyService
{
	Task<PagedResult<TradeStrategy>> GetStrategiesPagedAsync(int userId, StrategyFilterRequest? filterRequest, StrategySortRequest? sortRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
	Task<StrategyStatisticReadModel> GetStrategyStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<TradeStrategy> GetStrategyByIdAsync(int strategyId, CancellationToken cancellationToken);
	Task<PagedResult<UserStrategyTradeCode>> GetUserStrategyCodesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task CreateUserStrategyCodeAsync(UserStrategyTradeCodeCreateDto request, int userId, CancellationToken cancellationToken);
	Task DeleteUserStrategyCodeAsync(int strategyId, int tradeCodeId, int userId, CancellationToken cancellationToken);
	Task<PagedResult<UserTradeStrategy>> GetUserStrategiesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task CreateUserStrategyAsync(CreateUserStrategyCreateDto request, int userId, CancellationToken cancellationToken);
	Task DeleteUserStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken);
}

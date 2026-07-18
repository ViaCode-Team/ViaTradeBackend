using Application.Contracts.Dto.Requests.Trade;
using Application.Contracts.Dto.Statistic;
using Application.Contracts.Dto.Strategy;
using Domain.Entities.DataBase;
using Domain.Models.Filters;
using Domain.Models.Pagination;
using Domain.Models.Sort;

namespace Application.Interfaces;

public interface IStrategyService
{
	Task<PagedResult<TradeStrategyDto>> GetStrategiesPagedAsync(int userId, StrategyFilterRequest? filterRequest, StrategySortRequest? sortRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
	Task<StrategyStatisticDto> GetStrategyStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<TradeStrategy> GetStrategyByIdAsync(int strategyId, CancellationToken cancellationToken);
	Task<PagedResult<UserStrategyTradeCodeDto>> GetUserStrategyCodesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task CreateUserStrategyCodeAsync(UserStrategyTradeCodeCreateDto request, int userId, CancellationToken cancellationToken);
	Task DeleteUserStrategyCodeAsync(int strategyId, int tradeCodeId, int userId, CancellationToken cancellationToken);
	Task<PagedResult<UserTradeStrategyDto>> GetUserStrategiesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task CreateUserStrategyAsync(CreateUserStrategyCreateDto request, int userId, CancellationToken cancellationToken);
	Task DeleteUserStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken);
}

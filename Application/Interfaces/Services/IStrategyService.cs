using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Strategy;
using Domain.Models.Pagination;
using ViaTradeBackend.Models.Trade;

namespace Application.Interfaces;

public interface IStrategyService
{
	Task<PagedResult<TradeStrategyDto>> GetStrategiesPagedAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<StrategyStatistic> GetStrategyStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<TradeStrategy> GetStrategyByIdAsync(int strategyId, CancellationToken cancellationToken);
	Task<PagedResult<UserStrategyTradeCodeDto>> GetUserStrategyCodesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task CreateUserStrategyCodeAsync(UserStrategyTradeCodeRequest request, int userId, CancellationToken cancellationToken);
	Task DeleteUserStrategyCodeAsync(int strategyId, int tradeCodeId, int userId, CancellationToken cancellationToken);
	Task<PagedResult<UserTradeStrategyDto>> GetUserStrategiesPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task CreateUserStrategyAsync(CreateUserStrategyRequest request, int userId, CancellationToken cancellationToken);
	Task DeleteUserStrategyAsync(int strategyId, int userId, CancellationToken cancellationToken);
}

using Application.Contracts.Dto.Requests.Trade;
using Application.Contracts.Dto.Statistic;
using Application.Contracts.Dto.Trade;
using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeRepository : IRepository<Trade, TradeDto>
{
	Task<GlobalStatisticDto> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetPagedFilteredAsync(IQuerySpecification<Trade> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
	Task<int> UpdateUserTradeAsync(int id, int userId, TradeCreateDto request, double? netIncome, decimal price, CancellationToken cancellationToken = default);
}


using Application.Contracts.Dto.Statistic;
using Application.Contracts.Dto.Trade;
using Domain.Entities.DataBase;
using Domain.Models.Filters;
using Domain.Models.Pagination;

using Application.Contracts.Dto.Requests.Trade;

namespace Application.Interfaces;
public interface ITradeService
{
	Task<GlobalStatisticDto> GetGlobalStatisticAsync(int userId, CancellationToken cancellationToken);
	Task<PagedResult<TradeDto>> GetByUserPagedAsync(int userId, TradeFilterRequest? filterRequest, PaginationRequest? paginationRequest, CancellationToken cancellationToken);
	Task<Trade> GetTradeByIdAsync(int id, int userId, CancellationToken cancellationToken);
	Task<Trade> CreateTradeAsync(TradeCreateDto request, int userId, CancellationToken cancellationToken);
	Task<Trade> UpdateTradeAsync(int id, TradeCreateDto request, int userId, CancellationToken cancellationToken);
	Task DeleteTradeAsync(int id, int userId, CancellationToken cancellationToken);
}

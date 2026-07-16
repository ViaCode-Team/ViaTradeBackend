using Domain.Entities.DataBase;
using Domain.Interfaces;
using Domain.Models.Dto.Strategy;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeStrategyRepository : IRepository<TradeStrategy, TradeStrategyDto>
{
	Task<int> CountAsync(CancellationToken cancellationToken = default);
	Task<TradeStrategyDto?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
	Task<PagedResult<TradeStrategyDto>> GetPagedFilteredAsync(int userId, ISpecification<TradeStrategy> spec, PaginationRequest? paginationRequest, CancellationToken cancellationToken = default);
}

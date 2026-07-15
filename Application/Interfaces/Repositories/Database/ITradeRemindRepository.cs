using Domain.Entities.DataBase;
using Domain.Models.Dto.NoteRemind;
using Domain.Models.Pagination;

namespace Application.Interfaces.Repositories.Database;

public interface ITradeRemindRepository : IRepository<TradeRemind, TradeRemindDto>
{
	Task<IEnumerable<TradeRemind>> GetActualRemind(CancellationToken cancellationToken);
	Task<PagedResult<TradeRemindDto>> GetByUserPagedAsync(int userId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<PagedResult<TradeRemindDto>> GetByUserAndTradeCodePagedAsync(int userId, int tradeCodeId, PaginationRequest paginationRequest, CancellationToken cancellationToken);
	Task<int> CountByUserAsync(int userId, CancellationToken cancellationToken);
}

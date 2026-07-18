using Application.Models.Statistic;
using Application.Contracts.Dto.Trade;
using Domain.Entities.CSV;
using Domain.Entities.DataBase;
using Domain.Models.Pagination;
using Domain.Models.Sort;

namespace Application.Interfaces;

public interface ITradeCodeService
{
	Task<StockStatisticReadModel> GetStockStatisticAsync(CancellationToken ct = default);
	Task<PagedResult<TradeCode>> GetCodesPagedAsync(PaginationRequest paginationRequest, StockSortRequest? sortRequest = null, CancellationToken cancellationToken = default);
	Task<IEnumerable<TradeCodeFileDto>> GetSysAllCodesAsync(TradeDataType dataType, CancellationToken ct = default);
	Task<TradeCodeFileDto> GetSysCodeByIdAsync(TradeDataType dataType, string tradeIdString, CancellationToken ct = default);
}

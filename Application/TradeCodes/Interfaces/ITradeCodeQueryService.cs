using Application.Common.Models.Pagination;
using Application.Common.Models.Sort;
using Application.Statistics.Models;
using Application.Trades.Models;
using Domain.TradeCodes.Entities;
using Domain.Trades.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeQueryService
{
	Task<PagedResult<TradeCode>> GetAsync(PaginationRequest paginationRequest, StockSortRequest? sortRequest, CancellationToken ct);
	Task<StockStatisticReadModel> GetStatisticsAsync(CancellationToken ct);
	Task<IEnumerable<TradeCodeFileDto>> GetSystemAsync(TradeDataType dataType, CancellationToken ct);
	Task<TradeCodeFileDto> GetSystemAsync(TradeDataType dataType, string tradeIdString, CancellationToken ct);
}

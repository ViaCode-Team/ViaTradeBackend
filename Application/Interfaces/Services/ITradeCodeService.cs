using Domain.Entities.CSV;
using Domain.Entities.DataBase;
using Domain.Models.Dto.Statistic;
using Domain.Models.Dto.Trade;
using Domain.Models.Pagination;

namespace Application.Interfaces;

public interface ITradeCodeService
{
	Task<PagedResult<TradeCodeDto>> GetCodesPagedAsync(PaginationRequest paginationRequest, CancellationToken cancellationToken = default);
	Task<StockStatistic> GetStockStatisticAsync(CancellationToken ct = default);
	Task<IEnumerable<TradeCodeFileDto>> GetSysAllCodesAsync(TradeDataType dataType, CancellationToken ct = default);
	Task<TradeCodeFileDto> GetSysCodeByIdAsync(TradeDataType dataType, string tradeIdString, CancellationToken ct = default);
}

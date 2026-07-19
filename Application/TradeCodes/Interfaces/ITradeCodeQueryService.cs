using Application.Common.Queries;
using Application.Statistics.Models;
using Application.TradeCodes.Queries;
using Application.Trades.Models;
using Domain.TradeCodes.Entities;
using Domain.Trades.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeQueryService
{
	Task<PageResult<TradeCode>> GetAsync(PageOptions page, TradeCodeSort sort, CancellationToken ct);
	Task<StockStatisticReadModel> GetStatisticsAsync(CancellationToken ct);
	Task<IEnumerable<TradeCodeFileDto>> GetFileMetadataAsync(TradeDataType dataType, CancellationToken ct);
	Task<TradeCodeFileDto> GetFileMetadataAsync(TradeDataType dataType, string tradeIdString, CancellationToken ct);
}

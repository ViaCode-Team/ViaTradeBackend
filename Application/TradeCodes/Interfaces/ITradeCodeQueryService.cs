using Application.Common.Models;
using Application.TradeCodes.Models;
using Application.Trades.Models;
using Domain.Enums;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes.Interfaces;

public interface ITradeCodeQueryService
{
	Task<StockStatisticDto> GetStatisticsAsync(CancellationToken ct);
	Task<PageResult<TradeCode>> GetPageAsync(PageOptions page, TradeCodeSort sort, CancellationToken ct);
	Task<IReadOnlyList<TradeCodeFileDto>> ListFileMetadataAsync(TradeDataType dataType, CancellationToken ct);
	Task<TradeCodeFileDto> GetFileMetadataAsync(TradeDataType dataType, string tradeIdString, CancellationToken ct);
}

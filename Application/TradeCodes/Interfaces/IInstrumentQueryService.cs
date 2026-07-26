using Application.Common.Models;
using Application.TradeCodes.Models;
using Application.Trades.Models;
using Domain.Enums;
using Domain.TradeCodes.Entities;

namespace Application.TradeCodes.Interfaces;

public interface IInstrumentQueryService
{
	Task<InstrumentStatisticsDto> GetStatisticsAsync(CancellationToken ct);
	Task<TradeCode> GetAsync(int instrumentId, CancellationToken ct);
	Task<TradeCode> GetBySymbolAsync(string symbol, CancellationToken ct);
	Task<PageResult<TradeCode>> GetPageAsync(
		InstrumentFilter instrumentFilter,
		PageOptions pageOptions,
		InstrumentSort instrumentSort,
		CancellationToken ct
	);
	Task<IReadOnlyList<InstrumentFileDto>> ListFileMetadataAsync(TradeDataType dataType, CancellationToken ct);
	Task<InstrumentFileDto> GetFileMetadataAsync(
		TradeDataType dataType,
		string instrumentIdOrSymbol,
		CancellationToken ct
	);
}

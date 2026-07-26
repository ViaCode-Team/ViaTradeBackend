using Application.Common.Models;
using Application.Instruments.Models;
using Application.Trades.Models;
using Domain.Entities;
using Domain.Enums;

namespace Application.Instruments.Interfaces;

public interface IInstrumentQueryService
{
	Task<InstrumentStatisticsDto> GetStatisticsAsync(CancellationToken ct);
	Task<Instrument> GetAsync(int instrumentId, CancellationToken ct);
	Task<Instrument> GetBySymbolAsync(string symbol, CancellationToken ct);
	Task<PageResult<Instrument>> GetPageAsync(
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

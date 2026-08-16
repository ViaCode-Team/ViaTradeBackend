using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Instruments.Models;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Entities;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Application.Instruments.Interfaces;

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
	Task<PageResult<Instrument>> GetPageSearchAsync(
		SearchFilter instrumentFilter,
		PageOptions pageOptions,
		CancellationToken ct
	);
	Task<IReadOnlyList<InstrumentFileDto>> ListFileMetadataAsync(TradeDataType dataType, CancellationToken ct);
	Task<InstrumentFileDto> GetFileMetadataAsync(
		TradeDataType dataType,
		string instrumentIdOrSymbol,
		CancellationToken ct
	);
}

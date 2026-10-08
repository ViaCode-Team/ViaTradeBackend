using ViaTrade.Domain.Enums;
using ViaTrade.Domain.Models.Trade;

namespace ViaTrade.Application.Common.Abstractions;

public interface IFileReader
{
	/// <summary>
	/// Returns available instruments for the specified data type.
	/// If filterTickers is provided, returns only instruments from that list.
	/// </summary>
	IEnumerable<InstrumentFile> GetInstruments(TradeDataType dataType, IEnumerable<string>? filterTickers = null);

	/// <summary>
	/// Reads data for multiple instruments with optional date filtering.
	/// Files not found are skipped silently (logged if needed).
	/// </summary>
	IEnumerable<(string Ticker, T Item)> ReadDataByTickers<T>(
		TradeDataType dataType,
		IEnumerable<string> tickers,
		DateTime? startDate = null,
		DateTime? endDate = null
	)
		where T : class;

	IEnumerable<(string Ticker, string StrategyName, T Item)> ReadDataByTickersWithStrategy<T>(
		TradeDataType dataType,
		IEnumerable<string> tickers,
		DateTime? startDate = null,
		DateTime? endDate = null
	)
		where T : class;
}

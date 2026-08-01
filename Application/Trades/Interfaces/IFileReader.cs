using Domain.Enums;
using Domain.Models.Trade;

namespace Application.Trades.Interfaces;

public interface IFileReader
{
	/// <summary>
	/// Returns available instruments for the specified data type.
	/// If filterSymbols is provided, returns only instruments from that list.
	/// </summary>
	IEnumerable<InstrumentFile> GetInstruments(TradeDataType dataType, IEnumerable<string>? filterSymbols = null);

	/// <summary>
	/// Reads data for multiple instruments with optional date filtering.
	/// Files not found are skipped silently (logged if needed).
	/// </summary>
	IEnumerable<(string Symbol, T Item)> ReadDataBySymbols<T>(
		TradeDataType dataType,
		IEnumerable<string> symbols,
		DateTime? startDate = null,
		DateTime? endDate = null
	)
		where T : class;

	IEnumerable<(string Symbol, string StrategyName, T Item)> ReadDataBySymbolsWithStrategy<T>(
		TradeDataType dataType,
		IEnumerable<string> symbols,
		DateTime? startDate = null,
		DateTime? endDate = null
	)
		where T : class;
}

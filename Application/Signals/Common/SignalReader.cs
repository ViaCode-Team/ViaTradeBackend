using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Domain.Enums;
using ViaTrade.Domain.Models.Trade;

namespace ViaTrade.Application.Signals.Common;

public sealed class SignalReader(IFileReader tradefileReader)
{
	public List<SignalResult> ListSignals(
		List<SignalSource> sources,
		DateTime? startDate,
		DateTime? endDate,
		SignalSort signalSort
	)
	{
		if (sources.Count == 0)
			return [];

		startDate = GetDateOnly(startDate);
		endDate = GetDateOnly(endDate);

		var symbols = sources.Select(source => source.Symbol).Distinct().ToList();
		var sourceByKey = sources.ToDictionary(source => (source.StrategyName, source.Symbol));
		var results = tradefileReader.ReadDataBySymbolsWithStrategy<StrategyResult>(
			TradeDataType.Strategy,
			symbols,
			startDate,
			endDate
		);

		var signals = results
			.Where(result => result.Symbol != null && result.StrategyName != null)
			.Select(result => new
			{
				Result = result,
				Source = sourceByKey.GetValueOrDefault((result.StrategyName!, result.Symbol!)),
			})
			.Where(item => item.Source != null)
			.Select(item => new SignalResult(
				item.Source!.StrategyId,
				item.Source.StrategyName,
				item.Source.DisplayName,
				item.Source.InstrumentId,
				item.Source.Symbol,
				item.Source.Accuracy,
				item.Result.Item.Date,
				item.Result.Item.ClosePrice,
				item.Result.Item.Signal
			))
			.ToList();

		return ApplySorting(signals, signalSort.GetEffectiveSortBy()).ToList();
	}

	public List<SignalResult> ListLatestSignals(List<SignalSource> sources)
	{
		if (sources.Count == 0)
			return [];

		var symbols = sources.Select(source => source.Symbol).Distinct().ToList();
		var sourceByKey = sources.ToDictionary(source => (source.StrategyName, source.Symbol));
		var latestBySource = new Dictionary<(int StrategyId, int InstrumentId), SignalResult>();
		var results = tradefileReader.ReadDataBySymbolsWithStrategy<StrategyResult>(TradeDataType.Strategy, symbols);

		foreach (var result in results)
		{
			if (result.Symbol == null || result.StrategyName == null)
				continue;

			var source = sourceByKey.GetValueOrDefault((result.StrategyName, result.Symbol));
			if (source == null)
				continue;

			var signal = new SignalResult(
				source.StrategyId,
				source.StrategyName,
				source.DisplayName,
				source.InstrumentId,
				source.Symbol,
				source.Accuracy,
				result.Item.Date,
				result.Item.ClosePrice,
				result.Item.Signal
			);
			var key = (signal.StrategyId, signal.InstrumentId);
			var hasCurrentSignal = latestBySource.TryGetValue(key, out var current);

			if (!hasCurrentSignal)
			{
				latestBySource[key] = signal;
				continue;
			}

			if (signal.Date > current!.Date)
				latestBySource[key] = signal;
		}

		return latestBySource.Values.ToList();
	}

	private static DateTime? GetDateOnly(DateTime? date)
	{
		if (!date.HasValue)
			return null;

		return date.Value.Date;
	}

	public static List<SignalResult> ApplySignalFilter(List<SignalResult> signals, List<TradeSignal>? filterSignals)
	{
		if (filterSignals == null || filterSignals.Count == 0)
			return signals;

		var signalStrings = filterSignals.Select(s => s.ToString()).ToList();

		return signals
			.Where(signal => signalStrings.Contains(signal.Signal, StringComparer.OrdinalIgnoreCase))
			.ToList();
	}

	public static IEnumerable<SignalResult> ApplySorting(
		IEnumerable<SignalResult> signals,
		List<SignalSortField> sortFields
	)
	{
		IOrderedEnumerable<SignalResult>? orderedSignals = null;
		foreach (var field in sortFields)
		{
			orderedSignals = (orderedSignals, field) switch
			{
				(null, SignalSortField.SignalDateAsc) => signals.OrderBy(signal => signal.Date),
				(null, SignalSortField.SignalDateDesc) => signals.OrderByDescending(signal => signal.Date),
				(null, SignalSortField.SymbolAsc) => signals.OrderBy(signal => signal.Symbol),
				(null, SignalSortField.SymbolDesc) => signals.OrderByDescending(signal => signal.Symbol),
				(null, SignalSortField.AccuracyAsc) => signals.OrderBy(signal => signal.Accuracy),
				(null, SignalSortField.AccuracyDesc) => signals.OrderByDescending(signal => signal.Accuracy),
				(null, _) => signals.OrderByDescending(signal => signal.Date),
				(_, SignalSortField.SignalDateAsc) => orderedSignals.ThenBy(signal => signal.Date),
				(_, SignalSortField.SignalDateDesc) => orderedSignals.ThenByDescending(signal => signal.Date),
				(_, SignalSortField.SymbolAsc) => orderedSignals.ThenBy(signal => signal.Symbol),
				(_, SignalSortField.SymbolDesc) => orderedSignals.ThenByDescending(signal => signal.Symbol),
				(_, SignalSortField.AccuracyAsc) => orderedSignals.ThenBy(signal => signal.Accuracy),
				(_, SignalSortField.AccuracyDesc) => orderedSignals.ThenByDescending(signal => signal.Accuracy),
				(_, _) => orderedSignals.ThenByDescending(signal => signal.Date),
			};
		}

		if (orderedSignals == null)
			return ApplyStableOrder(signals.OrderByDescending(signal => signal.Date));

		return ApplyStableOrder(orderedSignals);
	}

	private static IOrderedEnumerable<SignalResult> ApplyStableOrder(IOrderedEnumerable<SignalResult> signals)
	{
		return signals
			.ThenBy(signal => signal.StrategyId)
			.ThenBy(signal => signal.InstrumentId)
			.ThenBy(signal => signal.Signal)
			.ThenBy(signal => signal.ClosePrice);
	}
}

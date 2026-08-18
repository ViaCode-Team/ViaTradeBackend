using ViaTrade.Application.Common.Exceptions;
using ViaTrade.Application.Common.Models;
using ViaTrade.Application.Strategies.Interfaces;
using ViaTrade.Application.Trades.Interfaces;
using ViaTrade.Application.Trades.Models;
using ViaTrade.Domain.Enums;
using ViaTrade.Domain.Models.Trade;

namespace ViaTrade.Application.Trades;

public class SignalQueryService(IFileReader tradefileReader, IUserStrategyRepository userStrategyRepository)
	: ISignalQueryService
{
	public async Task<SignalStatisticDto> GetStatisticsAsync(int userId, CancellationToken ct)
	{
		var sources = await userStrategyRepository.ListSignalSourcesAsync(userId, ct);
		var signals = ListSignals(sources, null, null, new SignalSort());

		return new SignalStatisticDto(
			signals.Count,
			signals.Count(signal => signal.Signal == "BUY"),
			signals.Count(signal => signal.Signal == "SELL")
		);
	}

	public async Task<PageResult<SignalDto>> GetPageAsync(
		int userId,
		SignalHistoryFilter signalHistoryFilter,
		SignalSort signalSort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var sources = await userStrategyRepository.ListSignalSourcesAsync(userId, ct);
		sources = sources
			.Where(source => source.StrategyId == signalHistoryFilter.StrategyId)
			.Where(source => source.InstrumentId == signalHistoryFilter.InstrumentId)
			.ToList();
		if (sources.Count == 0)
			throw new NotFoundException("Strategy instrument link was not found.", "strategy_instrument_not_found");

		var signals = ListSignals(sources, signalHistoryFilter.StartDate, signalHistoryFilter.EndDate, signalSort);

		signals = ApplySignalFilter(signals, signalHistoryFilter.Signals);

		return CreatePageResult(signals, pageOptions);
	}

	public async Task<PageResult<SignalDto>> GetLatestPageAsync(
		int userId,
		LatestSignalFilter latestSignalFilter,
		SignalSort signalSort,
		PageOptions pageOptions,
		CancellationToken ct
	)
	{
		var sources = await userStrategyRepository.ListSignalSourcesAsync(userId, ct);
		var signals = ListLatestSignals(sources);

		signals = ApplySignalFilter(signals, latestSignalFilter.Signals);

		signals = ApplySorting(signals, signalSort.GetEffectiveSortBy()).ToList();

		return CreatePageResult(signals, pageOptions);
	}

	private List<SignalDto> ListSignals(
		List<SignalSourceDto> sources,
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
			.Select(item => new SignalDto(
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

	private List<SignalDto> ListLatestSignals(List<SignalSourceDto> sources)
	{
		if (sources.Count == 0)
			return [];

		var symbols = sources.Select(source => source.Symbol).Distinct().ToList();
		var sourceByKey = sources.ToDictionary(source => (source.StrategyName, source.Symbol));
		var latestBySource = new Dictionary<(int StrategyId, int InstrumentId), SignalDto>();
		var results = tradefileReader.ReadDataBySymbolsWithStrategy<StrategyResult>(TradeDataType.Strategy, symbols);

		foreach (var result in results)
		{
			if (result.Symbol == null || result.StrategyName == null)
				continue;

			var source = sourceByKey.GetValueOrDefault((result.StrategyName, result.Symbol));
			if (source == null)
				continue;

			var signal = new SignalDto(
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

	private static PageResult<SignalDto> CreatePageResult(List<SignalDto> signals, PageOptions pageOptions)
	{
		var items = signals.Skip((pageOptions.Page - 1) * pageOptions.PageSize).Take(pageOptions.PageSize).ToList();

		return new PageResult<SignalDto>(items, signals.Count, pageOptions.Page, pageOptions.PageSize);
	}

	private static DateTime? GetDateOnly(DateTime? date)
	{
		if (!date.HasValue)
			return null;

		return date.Value.Date;
	}

	private static List<SignalDto> ApplySignalFilter(List<SignalDto> signals, List<TradeSignal>? filterSignals)
	{
		if (filterSignals == null || filterSignals.Count == 0)
			return signals;

		var signalStrings = filterSignals.Select(s => s.ToString()).ToList();

		return signals
			.Where(signal => signalStrings.Contains(signal.Signal, StringComparer.OrdinalIgnoreCase))
			.ToList();
	}

	private static IEnumerable<SignalDto> ApplySorting(IEnumerable<SignalDto> signals, List<SignalSortField> sortFields)
	{
		IOrderedEnumerable<SignalDto>? orderedSignals = null;
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

	private static IOrderedEnumerable<SignalDto> ApplyStableOrder(IOrderedEnumerable<SignalDto> signals)
	{
		return signals
			.ThenBy(signal => signal.StrategyId)
			.ThenBy(signal => signal.InstrumentId)
			.ThenBy(signal => signal.Signal)
			.ThenBy(signal => signal.ClosePrice);
	}
}

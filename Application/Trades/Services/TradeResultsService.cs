using Application.Auth.Interfaces;
using Application.Common.Models.Sort;
using Application.Interfaces;
using Application.Statistics.Models;
using Application.Strategies.Interfaces;
using Domain.Trades.Entities;
using Domain.Trades.Enums;

namespace Application.Trades.Services;

public class TradeResultsService(
	IFileReader tradefileReader,
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository) : ITradeResultsService
{
	public async Task<SignalStatisticReadModel> GetStrategyResultStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		var signals = await GetStrategyResultAsync(userId, DateTime.Now, null, null, cancellationToken);

		var allResults = signals.Strategies
			.SelectMany(s => s.Tickers)
			.SelectMany(t => t.Results);

		return new SignalStatisticReadModel
		{
			TotalSignals = allResults.Count(),
			BuySignals = allResults.Count(r => r.Signal == "BUY"),
			SellSignals = allResults.Count(r => r.Signal == "SELL")
		};
	}

	public async Task<StrategyResultResponse> GetStrategyResultAsync(
		int userId,
		DateTime? startDate,
		DateTime? endDate,
		SignalSortRequest? sortRequest,
		CancellationToken cancellationToken)
	{
		if (startDate != null)
		{
			startDate = startDate.Value.Date;
		}

		var strategys = await tradeStrategyRepository.GetAllAsync(cancellationToken);
		var userPreferences = await userTradeStrategyRepository.GetUserPreferencesAsync(userId, cancellationToken);
		if (userPreferences.Count == 0)
			return new StrategyResultResponse { Strategies = [] };

		List<(string StrategyName, int? Accuracy, string TradeCode, StrategyResult Item)> allResults = [];

		foreach (var kvp in userPreferences)
		{
			var strategyName = kvp.Key;
			var tradeCodes = kvp.Value;

			var accuracy = strategys.FirstOrDefault(s => s.Name == strategyName)?.Accuracy;

			var results = tradefileReader.ReadDataByCodesWithStrategy<StrategyResult>(
				TradeDataType.Strategy,
				tradeCodes,
				startDate,
				endDate);

			foreach (var result in results)
			{
				var code = result.TradeCode;
				var fileStrategy = result.StrategyName;
				var item = result.Item;

				if (!string.IsNullOrEmpty(code) && fileStrategy == strategyName)
				{
					allResults.Add((strategyName, accuracy, code, item));
				}
			}
		}

		var sortFields = sortRequest?.GetEffectiveSortBy() ?? [SignalSortField.DateTimeDesc];

		var strategies = allResults
			.GroupBy(x => x.StrategyName)
			.Select(g =>
			{
				var tickers = g
					.GroupBy(x => x.TradeCode)
					.Select(t => new TickerResults
					{
						TradeCode = t.Key,
						Results = ApplyResultSorting(t.Select(x => x.Item), sortFields).ToList(),
						Accuracy = t.Select(x => x.Accuracy).FirstOrDefault()
					});

				return new StrategyData
				{
					Name = g.Key,
					Tickers = ApplyTickerSorting(tickers, sortFields).ToList()
				};
			})
			.ToList();

		return new StrategyResultResponse { Strategies = strategies };
	}

	public async Task<StrategyResultResponse> GetStrategyResultByCodeAsync(
		int userId,
		string strategyName,
		string tradeCode,
		DateTime? startDate,
		DateTime? endDate,
		CancellationToken cancellationToken)
	{
		var userPreferences = await userTradeStrategyRepository.GetUserPreferencesAsync(userId, cancellationToken);
		var preference = userPreferences.FirstOrDefault(x => x.Key == strategyName);

		if (preference.Key == null || !preference.Value.Contains(tradeCode))
			throw new KeyNotFoundException();

		var strategy = await tradeStrategyRepository.GetByNameAsync(strategyName, cancellationToken);
		var accuracy = strategy?.Accuracy;

		var results = tradefileReader.ReadDataByCodesWithStrategy<StrategyResult>(
			TradeDataType.Strategy,
			[tradeCode],
			startDate,
			endDate);

		var filteredResults = results
			.Where(r => r.TradeCode == tradeCode && r.StrategyName == strategyName)
			.Select(r => r.Item)
			.ToList();

		return new StrategyResultResponse
		{
			Strategies =
			[
				new()
				{
					Name = strategyName,
					Tickers =
					[
						new()
						{
							TradeCode = tradeCode,
							Results = filteredResults,
							Accuracy = accuracy
						}
					]
				}
			]
		};
	}

	private static IEnumerable<StrategyResult> ApplyResultSorting(IEnumerable<StrategyResult> results, List<SignalSortField> sortFields)
	{
		if (sortFields.Count == 0)
			return results.OrderByDescending(r => r.Date);

		IOrderedEnumerable<StrategyResult>? orderedResults = null;
		foreach (var field in sortFields)
		{
			orderedResults = (orderedResults, field) switch
			{
				(null, SignalSortField.DateTimeAsc) => results.OrderBy(r => r.Date),
				(null, _) => results.OrderByDescending(r => r.Date),
				(_, SignalSortField.DateTimeAsc) => orderedResults.ThenBy(r => r.Date),
				(_, _) => orderedResults.ThenByDescending(r => r.Date)
			};
		}

		return orderedResults ?? results.OrderByDescending(r => r.Date);
	}

	private static IEnumerable<TickerResults> ApplyTickerSorting(IEnumerable<TickerResults> tickers, List<SignalSortField> sortFields)
	{
		if (sortFields.Count == 0)
			return tickers;

		IOrderedEnumerable<TickerResults>? orderedTickers = null;
		foreach (var field in sortFields)
		{
			orderedTickers = (orderedTickers, field) switch
			{
				(null, SignalSortField.AssetAsc) => tickers.OrderBy(t => t.TradeCode),
				(null, SignalSortField.AssetDesc) => tickers.OrderByDescending(t => t.TradeCode),
				(null, SignalSortField.AccuracyDesc) => tickers.OrderByDescending(t => t.Accuracy ?? 0),
				(null, SignalSortField.AccuracyAsc) => tickers.OrderBy(t => t.Accuracy ?? 0),
				(null, _) => tickers.OrderBy(t => 0),
				(_, SignalSortField.AssetAsc) => orderedTickers.ThenBy(t => t.TradeCode),
				(_, SignalSortField.AssetDesc) => orderedTickers.ThenByDescending(t => t.TradeCode),
				(_, SignalSortField.AccuracyDesc) => orderedTickers.ThenByDescending(t => t.Accuracy ?? 0),
				(_, SignalSortField.AccuracyAsc) => orderedTickers.ThenBy(t => t.Accuracy ?? 0),
				(_, _) => orderedTickers.ThenBy(t => 0)
			};
		}

		return orderedTickers ?? tickers;
	}
}

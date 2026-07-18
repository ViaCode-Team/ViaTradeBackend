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
	private readonly IFileReader _tradefileReader = tradefileReader;
	private readonly IUserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;

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

		var strategys = await _tradeStrategyRepository.GetAllAsync(cancellationToken);
		var userPreferences = await _userTradeStrategyRepository.GetUserPreferencesAsync(userId, cancellationToken);

		if (!userPreferences.Any())
			return new StrategyResultResponse { Strategies = [] };

		List<(string StrategyName, int? Accuracy, string TradeCode, StrategyResult Item)> allResults = [];

		foreach (var kvp in userPreferences)
		{
			var strategyName = kvp.Key;
			var tradeCodes = kvp.Value;

			var accuracy = strategys.FirstOrDefault(s => s.Name == strategyName)?.Accuracy;

			var results = _tradefileReader.ReadDataByCodesWithStrategy<StrategyResult>(
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

		var sortFields = sortRequest?.SortBy ?? [SignalSortField.DateTimeDesc];

		var strategies = allResults
			.GroupBy(x => x.StrategyName)
			.Select(g =>
			{
				var tickers = g
					.GroupBy(x => x.TradeCode)
					.Select(t =>
					{
						IEnumerable<StrategyResult> results = t.Select(x => x.Item);
						if (sortFields.Count > 0)
						{
							IOrderedEnumerable<StrategyResult>? orderedResults = null;
							foreach (var field in sortFields)
							{
								if (orderedResults == null)
								{
									orderedResults = field switch
									{
										SignalSortField.DateTimeAsc => results.OrderBy(r => r.Date),
										_ => results.OrderByDescending(r => r.Date)
									};
								}
								else
								{
									orderedResults = field switch
									{
										SignalSortField.DateTimeAsc => orderedResults.ThenBy(r => r.Date),
										_ => orderedResults.ThenByDescending(r => r.Date)
									};
								}
							}
							results = orderedResults ?? results.OrderByDescending(r => r.Date);
						}

						return new TickerResults
						{
							TradeCode = t.Key,
							Results = results.ToList(),
							Accuracy = t.Select(x => x.Accuracy).FirstOrDefault()
						};
					});

				if (sortFields.Count > 0)
				{
					IOrderedEnumerable<TickerResults>? orderedTickers = null;
					foreach (var field in sortFields)
					{
						if (orderedTickers == null)
						{
							orderedTickers = field switch
							{
								SignalSortField.AssetAsc => tickers.OrderBy(t => t.TradeCode),
								SignalSortField.AssetDesc => tickers.OrderByDescending(t => t.TradeCode),
								SignalSortField.AccuracyDesc => tickers.OrderByDescending(t => t.Accuracy ?? 0),
								SignalSortField.AccuracyAsc => tickers.OrderBy(t => t.Accuracy ?? 0),
								_ => tickers.OrderBy(t => 0)
							};
						}
						else
						{
							orderedTickers = field switch
							{
								SignalSortField.AssetAsc => orderedTickers.ThenBy(t => t.TradeCode),
								SignalSortField.AssetDesc => orderedTickers.ThenByDescending(t => t.TradeCode),
								SignalSortField.AccuracyDesc => orderedTickers.ThenByDescending(t => t.Accuracy ?? 0),
								SignalSortField.AccuracyAsc => orderedTickers.ThenBy(t => t.Accuracy ?? 0),
								_ => orderedTickers.ThenBy(t => 0)
							};
						}
					}
					tickers = orderedTickers ?? tickers;
				}

				return new StrategyData
				{
					Name = g.Key,
					Tickers = tickers.ToList()
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
		var userPreferences = await _userTradeStrategyRepository.GetUserPreferencesAsync(userId, cancellationToken);
		var preference = userPreferences.FirstOrDefault(x => x.Key == strategyName);

		if (preference.Key == null || !preference.Value.Contains(tradeCode))
			throw new KeyNotFoundException();

		var strategy = await _tradeStrategyRepository.GetByNameAsync(strategyName, cancellationToken);
		var accuracy = strategy?.Accuracy;

		var results = _tradefileReader.ReadDataByCodesWithStrategy<StrategyResult>(
			TradeDataType.Strategy,
			[tradeCode],
			startDate,
			endDate);

		List<StrategyResult> filteredResults = [];
		foreach (var (TradeCode, StrategyName, Item) in results)
		{
			if (TradeCode == tradeCode && StrategyName == strategyName)
			{
				filteredResults.Add(Item);
			}
		}

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
}

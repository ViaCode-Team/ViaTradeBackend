using Application.Interfaces;
using Application.Interfaces.Repositories.Database;
using Application.Interfaces.Utils;
using Domain.Entities.CSV;
using Domain.Enums;
using Domain.Models.Dto.Statistic;
using Domain.Models.Sort;
using Domain.Models.TradeLogic;
using Domain.Services;

namespace Application.Services;

public class TradeResultsService(
	IFileReader tradefileReader,
	IUserService userService,
	ITradeStrategyRepository tradeStrategyRepository,
	IUserTradeStrategyRepository userTradeStrategyRepository) : ITradeResultsService
{
	private readonly IFileReader _tradefileReader = tradefileReader;
	private readonly IUserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
	private readonly ITradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
	private readonly IUserService _userService = userService;

	public async Task<SignalStatistic> GetStrategyResultStatisticAsync(int userId, CancellationToken cancellationToken)
	{
		await _userService.EnsureUserAsync(userId, cancellationToken);

		var signals = await GetStrategyResultAsync(userId, DateTime.Now, null, null, cancellationToken);

		var allResults = signals.Strategies
			.SelectMany(s => s.Tickers)
			.SelectMany(t => t.Results);

		return new SignalStatistic
		{
			TotalSignals = SignalStatisticsCalcService.CountTotalSignals(allResults),
			BuySignals = SignalStatisticsCalcService.CountBuySignals(allResults),
			SellSignals = SignalStatisticsCalcService.CountSellSignals(allResults)
		};
	}

	public async Task<StrategyResultResponse> GetStrategyResultAsync(
		int userId,
		DateTime? startDate,
		DateTime? endDate,
		SignalSortRequest? sortRequest,
		CancellationToken cancellationToken)
	{
		// startDate work only with out time or with T00:00:00 time
		if (startDate != null)
		{
			startDate = startDate.Value.Date;
		}

		var strategys = await _tradeStrategyRepository.GetAllAsync(cancellationToken);
		var userPreferences = await _userTradeStrategyRepository.GetUserPreferencesAsync(userId, cancellationToken);

		if (!userPreferences.Any())
			return new StrategyResultResponse { Strategies = new List<StrategyData>() };

		var allResults = new List<(string StrategyName, int? Accuracy, string TradeCode, StrategyResult Item)>();

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

		var sortOrder = sortRequest?.SortOrder ?? SignalSortOrder.NewestFirst;

		var strategies = allResults
			.GroupBy(x => x.StrategyName)
			.Select(g =>
			{
				var tickers = g
					.GroupBy(x => x.TradeCode)
					.Select(t =>
					{
						var results = sortOrder switch
						{
							SignalSortOrder.OldestFirst => t.Select(x => x.Item).OrderBy(r => r.Date).ToList(),
							_ => t.Select(x => x.Item).OrderByDescending(r => r.Date).ToList()
						};

						return new TickerResults
						{
							TradeCode = t.Key,
							Results = results,
							Accuracy = t.Select(x => x.Accuracy).FirstOrDefault()
						};
					});

				tickers = sortOrder switch
				{
					SignalSortOrder.AssetAscending => tickers.OrderBy(t => t.TradeCode),
					SignalSortOrder.AssetDescending => tickers.OrderByDescending(t => t.TradeCode),
					SignalSortOrder.AccuracyDescending => tickers.OrderByDescending(t => t.Accuracy ?? 0),
					SignalSortOrder.AccuracyAscending => tickers.OrderBy(t => t.Accuracy ?? 0),
					_ => tickers
				};

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

		var filteredResults = new List<StrategyResult>();
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

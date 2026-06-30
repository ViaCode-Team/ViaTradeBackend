using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models.Dto.Statistic;
using Domain.Models.TradeLogic;
using Domain.Services;
using Infrastructure.Repositories.DataBase;

namespace Infrastructure.Services
{
    public class TradeResultsService(IFileReader tradefileReader, UserService userService, TradeStrategyRepository tradeStrategyRepository,
        UserTradeStrategyRepository userTradeStrategyRepository) : ITradeResultsService
    {
        private readonly IFileReader _tradefileReader = tradefileReader;
        private readonly UserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
        private readonly TradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
        private readonly UserService _userService = userService;
        public async Task<SignalStatistic> GetSignalStatisticAsync(int userId, CancellationToken cancellationToken)
        {
            await _userService.EnsureUserAsync(userId, cancellationToken);
            var signals = await GetStrategyResultAsync(userId, DateTime.Now, null, cancellationToken);

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
            CancellationToken cancellationToken)
        {
            // startDate work only with out time or with T00:00:00 time
            if (startDate != null)
            {
                startDate = startDate.Value.Date;
            }

            var strategys = await _tradeStrategyRepository.GetAllAsync();

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
                    string? code = result.Item1;
                    string fileStrategy = result.Item2;
                    StrategyResult item = result.Item3;

                    if (!string.IsNullOrEmpty(code) && fileStrategy == strategyName)
                    {
                        allResults.Add((strategyName, accuracy, code, item));
                    }
                }
            }

            return new StrategyResultResponse
            {
                Strategies = allResults
                    .GroupBy(x => x.StrategyName)
                    .Select(g => new StrategyData
                    {
                        Name = g.Key,
                        Tickers = g.GroupBy(x => x.TradeCode)
                                   .Select(t => new TickerResults
                                   {
                                       TradeCode = t.Key,
                                       Results = t.Select(x => x.Item).ToList(),
                                       Accuracy = t.Select(x => x.Accuracy).FirstOrDefault()
                                   }).ToList()
                    }).ToList()
            };
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
            {
                throw new KeyNotFoundException();
            }

            var strategy = await _tradeStrategyRepository.GetByNameAsync(strategyName, cancellationToken);
            var accuracy = strategy?.Accuracy;

            var results = _tradefileReader.ReadDataByCodesWithStrategy<StrategyResult>(
                TradeDataType.Strategy,
                [tradeCode],
                startDate,
                endDate);

            var filteredResults = new List<StrategyResult>();
            foreach (var result in results)
            {
                if (result.Item1 == tradeCode && result.Item2 == strategyName)
                {
                    filteredResults.Add(result.Item3);
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
}

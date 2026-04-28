using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models.TradeLogic;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositoryes.DataBase;

namespace Infrastructure.Services
{
    public class TradeResultsService(IFileReader tradefileReader, TradeStrategyRepository tradeStrategyRepository,
        UserTradeStrategyRepository userTradeStrategyRepository) : ITradeResultsService
    {
        private readonly IFileReader _tradefileReader = tradefileReader;
        private readonly UserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
        private readonly TradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;

        public async Task<StrategyResultResponse> GetStrategyResultAsync(
            int userId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken)
        {
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

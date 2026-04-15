using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models.TradeLogic;
using Infrastructure.Repositories.DataBase;

namespace Infrastructure.Services
{
    public class TradeResultsService(IFileReader tradefileReader, UserTradeStrategyRepository userTradeStrategyRepository) : ITradeResultsService
    {
        private readonly IFileReader _tradefileReader = tradefileReader;
        private readonly UserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;

        public async Task<StrategyResultResponse> GetStrategyResultAsync(
            int userId,
            DateTime? startDate,
            DateTime? endDate,
            CancellationToken cancellationToken)
        {
            var userPreferences = await _userTradeStrategyRepository.GetUserPreferencesAsync(userId, cancellationToken);

            if (!userPreferences.Any())
                return new StrategyResultResponse { Strategies = new List<StrategyData>() };

            var allResults = new List<(string StrategyName, string TradeCode, StrategyResult Item)>();

            foreach (var kvp in userPreferences)
            {
                var strategyName = kvp.Key;
                var tradeCodes = kvp.Value;

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
                        allResults.Add((strategyName, code, item));
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
                                       Results = t.Select(x => x.Item).ToList()
                                   }).ToList()
                    }).ToList()
            };
        }

    }
}

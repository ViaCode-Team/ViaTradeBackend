using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models.Dto;
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
            // 1. Получаем предпочтения пользователя (StrategyName -> List<TradeCode>)
            var userPreferences = await _userTradeStrategyRepository.GetUserPreferencesAsync(userId, cancellationToken);

            if (!userPreferences.Any())
                return new StrategyResultResponse { Strategies = new List<StrategyData>() };

            var allResults = new List<(string StrategyName, string TradeCode, StrategyResult Item)>();

            // 2. Process each strategy separately (логика без изменений)
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
                    // 3. Корректная деконструкция: первый элемент может быть null
                    string? code = result.Item1;
                    string fileStrategy = result.Item2;
                    StrategyResult item = result.Item3;

                    // Пропускаем записи без кода и фильтруем по имени стратегии
                    if (!string.IsNullOrEmpty(code) && fileStrategy == strategyName)
                    {
                        allResults.Add((strategyName, code, item));
                    }
                }
            }

            // 4. Build final response (без изменений)
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


        //public StrategyResultResponse GetStrategyResult(
        //    Dictionary<string, List<string>> userPreferences,
        //    DateTime? startDate,
        //    DateTime? endDate)
        //{
        //    var allResults = new List<(string StrategyName, string TradeCode, StrategyResult Item)>();

        //    // Process each strategy separately as requested
        //    foreach (var kvp in userPreferences)
        //    {
        //        var strategyName = kvp.Key;
        //        var tradeCodes = kvp.Value;

        //        var results = _tradefileReader.ReadDataByCodesWithStrategy<StrategyResult>(
        //            TradeDataType.Strategy,
        //            tradeCodes,
        //            startDate,
        //            endDate);

        //        // Filter only results that match the current strategy
        //        foreach (var (code, fileStrategy, item) in results)
        //        {
        //            if (fileStrategy == strategyName)
        //                allResults.Add((strategyName, code, item));
        //        }
        //    }

        //    // Build final response
        //    return new StrategyResultResponse
        //    {
        //        Strategies = allResults
        //            .GroupBy(x => x.StrategyName)
        //            .Select(g => new StrategyData
        //            {
        //                Name = g.Key,
        //                Tickers = g.GroupBy(x => x.TradeCode)
        //                           .Select(t => new TickerResults
        //                           {
        //                               TradeCode = t.Key,
        //                               Results = t.Select(x => x.Item).ToList()
        //                           }).ToList()
        //            }).ToList()
        //    };
        //}

    }
}
